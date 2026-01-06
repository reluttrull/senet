using Microsoft.AspNetCore.SignalR;
using SenetServer.Shared;

namespace SenetServer.SignalR
{
    public class NotificationHub : Hub
    {
        private readonly IUserConnectionManager _connectionManager;

        public NotificationHub(IUserConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public override Task OnConnectedAsync()
        {
            var http = Context.GetHttpContext();

            // Prefer cookie-based user id that was created earlier in the pipeline.
            if (http?.Request.Cookies.TryGetValue(UserIdentity.CookieName, out var existing) == true
                && System.Guid.TryParse(existing, out _))
            {
                _connectionManager.RegisterConnection(existing, Context.ConnectionId);
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception? exception)
        {
            _connectionManager.UnregisterConnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}