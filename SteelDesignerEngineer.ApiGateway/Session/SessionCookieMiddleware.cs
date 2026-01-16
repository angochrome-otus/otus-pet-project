using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Session;

public class SessionCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionCookieMiddleware> _logger;
    private const string SessionCookieName = "SDE_SessionId";

    public SessionCookieMiddleware(RequestDelegate next, ILogger<SessionCookieMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISessionServiceClient sessionClient)
    {
        try
        {
            if (context.Request.Cookies.TryGetValue(SessionCookieName, out var sessionId) && !string.IsNullOrWhiteSpace(sessionId))
            {
                var res = await sessionClient.ValidateSessionAsync(new ValidateSessionRequest { SessionId = sessionId });

                if (res != null && res.IsValid && !string.IsNullOrWhiteSpace(res.UserId))
                {
                    context.Items["UserId"] = res.UserId;
                    context.Items["SessionId"] = sessionId;
                }
                else
                {
                    DeleteSessionCookie(context);
                }
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SessionCookieMiddleware");
            await _next(context);
        }
    }

    public static void SetSessionCookie(HttpContext context, string sessionId, TimeSpan expiration)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.Add(expiration),
            Path = "/"
        };

        context.Response.Cookies.Append(SessionCookieName, sessionId, cookieOptions);
    }

    public static void DeleteSessionCookie(HttpContext context)
    {
        context.Response.Cookies.Delete(SessionCookieName, new CookieOptions
        {
            Path = "/",
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });
    }
}
