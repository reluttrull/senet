using System;
using Microsoft.AspNetCore.SignalR;
using SenetServer.Shared;

namespace SenetServer.SignalR
{
    // maps SignalR connection to UserId.
    public class UserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var http = connection.GetHttpContext();

            // prefer cookie-based UserId created by UserIdentity helper
            if (http?.Request.Cookies.TryGetValue(UserIdentity.CookieName, out var cookieId) == true
                && Guid.TryParse(cookieId, out _))
            {
                return cookieId;
            }

            // fallback: allow client to pass userId in query string (e.g. ?userId=...)
            if (http?.Request.Query.TryGetValue("userId", out var qvals) == true
                && !string.IsNullOrWhiteSpace(qvals) && Guid.TryParse(qvals.ToString(), out _))
            {
                return qvals.ToString();
            }

            // No valid user id available
            return null;
        }
    }
}