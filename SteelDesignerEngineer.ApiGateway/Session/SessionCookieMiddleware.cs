using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Session;

public class SessionCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionCookieMiddleware> _logger;

    // Must match Cookie.Name configured in Program.cs (AddSession)
    private const string SessionCookieName = ".SteelDesignerEngineer.Session";

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
                    context.Items["UserRole"] = res.UserRole;
                    context.Items["UserEmail"] = res.UserEmail;
                    context.Items["UserName"] = res.UserName;
                }
                else
                {
                    // Do not delete cookie on null/invalid response (can be transient)
                    // We only treat the request as unauthenticated.
                    context.Items.Remove("UserId");
                    context.Items.Remove("SessionId");
                    context.Items.Remove("UserRole");
                    context.Items.Remove("UserEmail");
                    context.Items.Remove("UserName");
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
        var secure = !context.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                     && context.Request.IsHttps;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.Add(expiration),
            Path = "/"
        };

        context.Response.Cookies.Append(SessionCookieName, sessionId, cookieOptions);
    }

    public static void DeleteSessionCookie(HttpContext context)
    {
        var secure = !context.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                     && context.Request.IsHttps;

        context.Response.Cookies.Delete(SessionCookieName, new CookieOptions
        {
            Path = "/",
            Secure = secure,
            SameSite = SameSiteMode.Lax
        });
    }
}
