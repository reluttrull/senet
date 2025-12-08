using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders.Physical;
using SenetServer.Matchmaking;
using SenetServer.Model;
using SenetServer.Shared;
using SenetServer.SignalR;
using System;

namespace SenetServer.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMatchmakingQueue _matchmakingQueue;
        private readonly IMemoryCache _memoryCache;

        public GameController(
            ILogger<GameController> logger,
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
        [Route("requestjoingame")]
        public async Task<IActionResult> RequestJoinGame()
        {
            var userId = UserIdentity.GetOrCreateUserId(HttpContext);

            string userName = UsernameGenerator.GetNewUsername();

            var request = new MatchRequest
            {
                UserId = userId,
                UserName = userName ?? $"Anonymous {new Random().Next(10000)}",
                TimeAdded = DateTime.UtcNow
            };

            await _matchmakingQueue.EnqueueAsync(request);
            _logger.LogInformation("Enqueued match request for user {UserId}: {UserName}", userId, userName);

            // return userId for SignalR notifications and userName to display
            return Ok(new { UserId = userId, UserName = userName });
        }

        [HttpGet]
        [Route("rollsticks")]
        public async Task<IActionResult> RollSticks()
        {
            var userId = UserIdentity.GetOrCreateUserId(HttpContext);

            if (!_memoryCache.TryGetValue(userId, out GameState? gameState)) return NotFound("Game not found.");
            if (gameState is null) return NotFound("Game missing data.");
            gameState.BoardState.RollSticks();
            await _hubContext.Clients.Users([gameState.PlayerWhite.UserId, gameState.PlayerBlack.UserId])
                .SendAsync("BoardUpdated", gameState.BoardState);

            return Ok();
        }
    }
}