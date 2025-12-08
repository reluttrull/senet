using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using SenetServer.Model;
using SenetServer.SignalR;
using System.Diagnostics;

namespace SenetServer.Matchmaking
{
    public class MatchmakingService : BackgroundService
    {
        private readonly IMatchmakingQueue _matchmakingQueue;
        private readonly ILogger<MatchmakingService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUserConnectionManager _connectionManager;
        private readonly IMemoryCache _memoryCache;


        public MatchmakingService(
            IMatchmakingQueue matchmakingQueue,
            ILogger<MatchmakingService> logger,
            IHubContext<NotificationHub> hubContext,
            IUserConnectionManager connectionManager,
            IMemoryCache memoryCache)
        {
            _matchmakingQueue = matchmakingQueue;
            _logger = logger;
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _memoryCache = memoryCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Matchmaking Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var first = await _matchmakingQueue.DequeueAsync(stoppingToken);
                    _logger.LogInformation("Dequeued first match request for user {UserId}: {UserName} (queued at {TimeAdded})", first.UserId, first.UserName, first.TimeAdded);

                    var second = await _matchmakingQueue.DequeueAsync(stoppingToken);
                    _logger.LogInformation("Dequeued second match request for user {UserId}: {UserName} (queued at {TimeAdded})", second.UserId, second.UserName, second.TimeAdded);

                    await ProcessMatchmakingPairAsync(first, second, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing matchmaking request.");
                }
            }
        }

        private async Task ProcessMatchmakingPairAsync(MatchRequest a, MatchRequest b, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Paired users {A}: {AName} and {B}: {BName}", a.UserId, a.UserName, b.UserId, b.UserName);

            var gameState = new GameState(new User(a.UserId, a.UserName), new User(b.UserId, b.UserName));
            var matchResponse = new MatchResponse()
            {
                PlayerWhite = gameState.PlayerWhite,
                PlayerBlack = gameState.PlayerBlack,
                TimeMatched = DateTime.UtcNow
            };

            // wait for both users to have an active SignalR connection (with a timeout).
            var timeout = TimeSpan.FromSeconds(10);
            var pollInterval = TimeSpan.FromMilliseconds(200);
            var sw = Stopwatch.StartNew();

            while (!stoppingToken.IsCancellationRequested && sw.Elapsed < timeout)
            {
                if (_connectionManager.HasConnections(a.UserId) && _connectionManager.HasConnections(b.UserId))
                {
                    break;
                }

                await Task.Delay(pollInterval, stoppingToken);
            }

            // todo: if only one was connected, add that user back into the queue

            if (!_connectionManager.HasConnections(a.UserId) || !_connectionManager.HasConnections(b.UserId))
            {
                _logger.LogWarning("One or both users are not connected for users {A} and {B}. Sending only to connected users if any.", a.UserId, b.UserId);

                var connected = new List<string>();
                if (_connectionManager.HasConnections(a.UserId)) connected.Add(a.UserId);
                if (_connectionManager.HasConnections(b.UserId)) connected.Add(b.UserId);

                if (connected.Count == 0)
                {
                    _logger.LogWarning("No active SignalR connections for users {A} or {B}. MatchResponse will not be sent via SignalR.", a.UserId, b.UserId);
                    return;
                }

                try
                {
                    await _hubContext.Clients.Users(connected)
                        .SendAsync("MatchFound", matchResponse, cancellationToken: stoppingToken);
                    await _hubContext.Clients.Users(connected)
                        .SendAsync("BoardUpdated", gameState.BoardState, cancellationToken: stoppingToken);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(3));
                    _memoryCache.Set(a.UserId, gameState, cacheEntryOptions);
                    _memoryCache.Set(b.UserId, gameState, cacheEntryOptions);
                    _logger.LogInformation("Sent MatchResponse to connected subset of users {Users}.", string.Join(",", connected));
                }
                catch (OperationCanceledException)
                {
                    // todo: handle cancellation request
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send MatchResponse to subset of users {A} and {B}.", a.UserId, b.UserId);
                }

                return;
            }

            // both connected — send to both
            try
            {
                var users = new[] { a.UserId, b.UserId };
                await _hubContext.Clients.Users(users)
                    .SendAsync("MatchFound", matchResponse, cancellationToken: stoppingToken);
                await _hubContext.Clients.Users(users)
                    .SendAsync("BoardUpdated", gameState.BoardState, cancellationToken: stoppingToken);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(3));
                _memoryCache.Set(a.UserId, gameState, cacheEntryOptions);
                _memoryCache.Set(b.UserId, gameState, cacheEntryOptions);
                _logger.LogInformation("Sent MatchResponse to users {A} and {B}.", a.UserId, b.UserId);
            }
            catch (OperationCanceledException)
            {
                // todo: handle cancellation request
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send MatchResponse to users {A} and {B}.", a.UserId, b.UserId);
            }
        }
    }
}
