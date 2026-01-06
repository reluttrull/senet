using Microsoft.AspNetCore.Http;
using System;

namespace SenetServer.Shared
{
    public static class UserIdentity
    {
        public const string CookieName = "SenetUserId";

        public static string GetOrCreateUserId(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            if (httpContext.Request.Cookies.TryGetValue(CookieName, out var existing) && Guid.TryParse(existing, out _))
            {
                return existing;
            }

            var newId = Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None, 
                Secure = true,                 
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            };

            // do not attempt to write headers if the response has already started
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.Cookies.Append(CookieName, newId, cookieOptions);
            }
            return newId;
        }
    }
}