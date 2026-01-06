using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using SenetServer.Application.ComputerOpponent;
using SenetServer.Contracts.Requests;
using SenetServer.Mapping;
using SenetServer.Matchmaking;
using SenetServer.Model;
using SenetServer.Shared;
using SenetServer.SignalR;

namespace SenetServer.Controllers
{
    [ApiController]
    [Route("singleplayer")]
    public class SingleplayerController : ControllerBase
    {
        private readonly ILogger<MultiplayerController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IComputerOpponentQueue _computerOpponentQueue;
        private readonly IMemoryCache _memoryCache;

        public SingleplayerController(
            ILogger<MultiplayerController> logger,
            IHubContext<NotificationHub> hubContext,
            IComputerOpponentQueue computerOpponentQueue,
            IMemoryCache memoryCache)
        {
            _logger = logger;
            _hubContext = hubContext;
            _computerOpponentQueue = computerOpponentQueue;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        [Route("games")]
        public async Task<IActionResult> RequestJoinGame([FromBody] StartGameRequest request)
        {
            User user = request.MapToUser();
            if (user.UserId == string.Empty || user.UserName == string.Empty) // todo: add proper validation
            {
                user.UserId = UserIdentity.GetOrCreateUserId(HttpContext);
                user.UserName = UsernameGenerator.GetNewUsername() ?? $"Anonymous{new Random().Next(10000)}";
            }

            _logger.LogInformation("Starting singleplayer game for user {UserId}: {UserName}", user.UserId, user.UserName);
            var gameState = new GameState(user, new User(string.Empty, Constants.ComputerOpponentName));
            var matchResponse = new MatchResponse()
            {
                PlayerWhite = gameState.PlayerWhite,
                PlayerBlack = gameState.PlayerBlack,
                TimeMatched = DateTime.UtcNow
            };
            await _hubContext.Clients.User(user.UserId)
                .SendAsync("ComputerMatchFound", matchResponse);
            await _hubContext.Clients.User(user.UserId)
                .SendAsync("BoardUpdated", gameState.BoardState);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(3));
            _memoryCache.Set(user.UserId, gameState, cacheEntryOptions);
            _logger.LogDebug("Sent MatchResponse to singleplayer user {userId}.", user.UserId);

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
            if (gameState.BoardState.IsWhiteTurn == (gameState.PlayerWhite.UserName == Constants.ComputerOpponentName))
            {
                await _computerOpponentQueue.EnqueueAsync(gameState);
                _logger.LogInformation("Turned over game to computer opponent for user {userId}.", userId);
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

            await _hubContext.Clients.User(userId)
                .SendAsync("BoardUpdated", gameState.BoardState);

            // if computer's turn, start moving
            if (gameState.BoardState.IsWhiteTurn == (gameState.PlayerWhite.UserName == Constants.ComputerOpponentName))
            {
                await _computerOpponentQueue.EnqueueAsync(gameState);
                _logger.LogInformation("Turned over game to computer opponent for user {userId}.", userId);
            }

            return Ok();
        }

        [HttpDelete]
        [Route("game/{userId}")]
        public async Task<IActionResult> QuitGame(string userId)
        {
            if (!_memoryCache.TryGetValue(userId, out GameState? gameState)) return NotFound("Game not found.");
            if (gameState is null) return NotFound("Game missing data.");

            _logger.LogInformation("User {userId} quitting game.", userId);
            await _hubContext.Clients.User(userId)
                .SendAsync("GameOver", gameState.PlayerWhite.UserId == userId ? gameState.PlayerBlack : gameState.PlayerWhite);
            _memoryCache.Remove(userId);

            return Ok();
        }
    }
}