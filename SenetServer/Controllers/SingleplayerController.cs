using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using SenetServer.Matchmaking;
using SenetServer.Model;
using SenetServer.SignalR;

namespace SenetServer.Controllers
{
    [ApiController]
    [Route("singleplayer")]
    public class SingleplayerController : ControllerBase
    {
        private readonly ILogger<MultiplayerController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMatchmakingQueue _matchmakingQueue;
        private readonly IMemoryCache _memoryCache;

        public SingleplayerController(
            ILogger<MultiplayerController> logger,
            IHubContext<NotificationHub> hubContext,
            IMatchmakingQueue matchmakingQueue,
            IMemoryCache memoryCache)
        {
            _logger = logger;
            _hubContext = hubContext;
            _matchmakingQueue = matchmakingQueue;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        [Route("games/{userName}/{userId}")]
        public async Task<IActionResult> RequestJoinGame(string userName, string userId)
        {
            _logger.LogInformation("Starting singleplayer game for user {UserId}: {UserName}", userId, userName);

            var gameState = new GameState(new User(userId, userName), new User(string.Empty, Constants.ComputerOpponentName));
            var matchResponse = new MatchResponse()
            {
                PlayerWhite = gameState.PlayerWhite,
                PlayerBlack = gameState.PlayerBlack,
                TimeMatched = DateTime.UtcNow
            };
            await _hubContext.Clients.User(userId)
                .SendAsync("MatchFound", matchResponse);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(3));
            _memoryCache.Set(userId, gameState, cacheEntryOptions);
            _logger.LogInformation("Sent MatchResponse to singleplayer user {userId}.", string.Join(",", userId));

            return Ok();
        }

        [HttpPut]
        [Route("sticks/{userId}")]
        public async Task<IActionResult> RollSticks(string userId)
        {
            if (!_memoryCache.TryGetValue(userId, out GameState? gameState)) return NotFound("Game not found.");
            if (gameState is null) return NotFound("Game missing data.");

            gameState.BoardState.RollSticks();
            await _hubContext.Clients.User(userId)
                .SendAsync("BoardUpdated", gameState.BoardState);

            return Ok();
        }

        [HttpPut]
        [Route("pawns/{userId}/{startPosition}")]
        public async Task<IActionResult> MovePawn(string userId, int startPosition)
        {
            if (!_memoryCache.TryGetValue(userId, out GameState? gameState)) return NotFound("Game not found.");
            if (gameState is null) return NotFound("Game missing data.");

            bool success = gameState.BoardState.MovePawn(startPosition);
            if (!success) return NotFound("Pawn not found.");

            gameState.BoardState.RollSticks();
            await _hubContext.Clients.User(userId)
                .SendAsync("BoardUpdated", gameState.BoardState);

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

            // if computer's turn, start moving

            return Ok();
        }

        [HttpPut]
        [Route("turns/{userId}/{isWhiteTurn}")]
        public async Task<IActionResult> ChangeTurn(string userId, bool isWhiteTurn)
        {
            if (!_memoryCache.TryGetValue(userId, out GameState? gameState)) return NotFound("Game not found.");
            if (gameState is null) return NotFound("Game missing data.");

            bool nextTurnIsWhiteTurn = !gameState.BoardState.IsWhiteTurn;
            gameState.BoardState.RollSticks();
            gameState.BoardState.IsWhiteTurn = nextTurnIsWhiteTurn;
            gameState.BoardState.SetCanMove();

            await _hubContext.Clients.User(userId)
                .SendAsync("BoardUpdated", gameState.BoardState);

            // if computer's turn, start moving

            return Ok();
        }
    }
}