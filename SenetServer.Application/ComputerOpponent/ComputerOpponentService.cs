using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenetServer.Matchmaking;
using SenetServer.Model;
using SenetServer.SignalR;

namespace SenetServer.Application.ComputerOpponent
{
    public class ComputerOpponentService : BackgroundService
    {
        private readonly IComputerOpponentQueue _computerOpponentQueue;
        private readonly ILogger<MatchmakingService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMemoryCache _memoryCache;

        public ComputerOpponentService(
            IComputerOpponentQueue computerOpponentQueue,
            ILogger<MatchmakingService> logger,
            IHubContext<NotificationHub> hubContext,
            IMemoryCache memoryCache)
        {
            _computerOpponentQueue = computerOpponentQueue;
            _logger = logger;
            _hubContext = hubContext;
            _memoryCache = memoryCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Computer opponent service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var gameState = await _computerOpponentQueue.DequeueAsync(stoppingToken);
                    // todo: human player won't always be white
                    _logger.LogInformation("Computer opponent equeued next game against user {UserId}: {UserName}", gameState.PlayerWhite.UserId, gameState.PlayerWhite.UserName);

                    await PlayTurnAsync(gameState, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred retrieving next game for computer opponent to play.");
                }
            }
        }

        private async Task PlayTurnAsync(GameState gameState, CancellationToken stoppingToken)
        {
            Console.WriteLine("playing turn in game");
            await Task.Delay(1000);
        }
    }
}