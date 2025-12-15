using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using SenetServer.Mapping;
using SenetServer.Matchmaking;
using SenetServer.Model;
using SenetServer.Shared;
using SenetServer.SignalR;

namespace SenetServer.Controllers
{
    [ApiController]
    [Route("multiplayer")]
    public class MultiplayerController : ControllerBase
    {
        private readonly ILogger<MultiplayerController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMatchmakingQueue _matchmakingQueue;
        private readonly IMemoryCache _memoryCache;

        public MultiplayerController(
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
        [Route("games")]
        public async Task<IActionResult> RequestJoinGame()
        {
            var userId = UserIdentity.GetOrCreateUserId(HttpContext);

            string userName = UsernameGenerator.GetNewUsername() ?? $"Anonymous{new Random().Next(10000)}";

            var request = new MatchRequest
            {
                UserId = userId,
                UserName = userName,
                TimeAdded = DateTime.UtcNow
            };

            await _matchmakingQueue.EnqueueAsync(request);
            _logger.LogInformation("Enqueued match request for user {UserId}: {UserName}", userId, userName);

            UserInfo userInfo = new UserInfo()
            {
                UserId = userId,
                UserName = userName
            };
            // return 202 with userId for SignalR notifications and userName for display
            // meanwhile, background service still has to process matches in queue
            return Accepted(ContractMapping.MapToResponse(userInfo));
        }

        [HttpPut]
        [Route("sticks/{userId}")]
        public async Task<IActionResult> RollSticks(string userId)
        {
            if (!_memoryCache.TryGetValue(userId, out GameState? gameState)) return NotFound("Game not found.");
            if (gameState is null) return NotFound("Game missing data.");

            gameState.BoardState.RollSticks();
            await _hubContext.Clients.Users([gameState.PlayerWhite.UserId, gameState.PlayerBlack.UserId])
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
            await _hubContext.Clients.Users([gameState.PlayerWhite.UserId, gameState.PlayerBlack.UserId])
                .SendAsync("BoardUpdated", gameState.BoardState);

            if (!gameState.BoardState.WhitePositions.Any(p => p < 30))
            {
                await _hubContext.Clients.Users([gameState.PlayerWhite.UserId, gameState.PlayerBlack.UserId])
                    .SendAsync("GameOver", gameState.PlayerWhite);
                _memoryCache.Remove(gameState.PlayerWhite.UserId);
                _memoryCache.Remove(gameState.PlayerBlack.UserId);
            }
            if (!gameState.BoardState.BlackPositions.Any(p => p < 30))
            {
                await _hubContext.Clients.Users([gameState.PlayerWhite.UserId, gameState.PlayerBlack.UserId])
                    .SendAsync("GameOver", gameState.PlayerBlack);
                _memoryCache.Remove(gameState.PlayerWhite.UserId);
                _memoryCache.Remove(gameState.PlayerBlack.UserId);
            }

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

            await _hubContext.Clients.Users([gameState.PlayerWhite.UserId, gameState.PlayerBlack.UserId])
                .SendAsync("BoardUpdated", gameState.BoardState);

            return Ok();
        }
    }
}