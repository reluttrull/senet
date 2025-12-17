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
                    _logger.LogDebug("Computer opponent queued next game against user {UserId}: {UserName}", gameState.PlayerWhite.UserId, gameState.PlayerWhite.UserName);

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
            bool isComputerWhite = gameState.PlayerWhite.UserName == Constants.ComputerOpponentName;
            string humanUserId = isComputerWhite ? gameState.PlayerBlack.UserId : gameState.PlayerWhite.UserId;
            _logger.LogDebug("Computer opponent playing turn in game against user {humanUserId}.", humanUserId);
            Random rand = new();
            bool stillMoving = true;
            
            while (stillMoving)
            {
                await Task.Delay(1000, stoppingToken);
                stillMoving = gameState.BoardState.SticksValue is 1 or 4 or 5;
                if (gameState.BoardState.MovablePositions.Count > 0)
                {
                    int randomPawn = gameState.BoardState.MovablePositions.ElementAt(rand.Next(0, gameState.BoardState.MovablePositions.Count));
                    _logger.LogDebug("Computer opponent moving pawn {randomPawn} by {sticks} spaces in game against user {humanUserId}", 
                        randomPawn, gameState.BoardState.SticksValue, humanUserId);
                    gameState.BoardState.MovePawn(randomPawn);
                    gameState.BoardState.RollSticks();
                }
                else
                {
                    bool nextTurnIsWhiteTurn = !gameState.BoardState.IsWhiteTurn;
                    gameState.BoardState.RollSticks();
                    gameState.BoardState.IsWhiteTurn = nextTurnIsWhiteTurn;
                    gameState.BoardState.SetCanMove();
                    _logger.LogDebug("Computer opponent skipping turn in game against user {humanUserId}", humanUserId);
                    stillMoving = false;
                }
                _logger.LogDebug("Computer opponent rolled {lastRoll} in game against user {humanUserId}", gameState.BoardState.SticksValue, humanUserId);

                await _hubContext.Clients.User(humanUserId)
                    .SendAsync("BoardUpdated", gameState.BoardState);

                await CheckGameOver(gameState, humanUserId);
                //stillMoving = stillMoving && gameState.BoardState.SticksValue is 1 or 4 or 5;
            }

            _logger.LogDebug("Computer opponent finished playing turn in game against user {humanUserId}", humanUserId);
        }

        private async Task CheckGameOver(GameState gameState, string userId)
        {
            if (!gameState.BoardState.WhitePositions.Any(p => p < 30))
            {
                await _hubContext.Clients.User(userId)
                    .SendAsync("GameOver", gameState.PlayerWhite);
                _memoryCache.Remove(userId);
            }
            if (!gameState.BoardState.BlackPositions.Any(p => p < 30))
            {
                await _hubContext.Clients.User(userId)
                    .SendAsync("GameOver", gameState.PlayerBlack);
                _memoryCache.Remove(userId);
            }
        }
    }
}