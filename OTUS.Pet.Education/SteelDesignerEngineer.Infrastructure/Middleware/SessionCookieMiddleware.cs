using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SteelDesignerEngineer.Domain.Services;

namespace SteelDesignerEngineer.Infrastructure.Middleware;

/// <summary>
/// Middleware для аутентификации через session cookies через Redis
/// Clean Architecture: Infrastructure Layer
/// </summary>
public class SessionCookieMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionCookieMiddleware> _logger;
    private const string SessionCookieName = "SDE_SessionId";

    public SessionCookieMiddleware(
        RequestDelegate next,
        ILogger<SessionCookieMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISessionService sessionService)
    {
        try
        {
            // Проверяем наличие session cookie
            if (context.Request.Cookies.TryGetValue(SessionCookieName, out var sessionId))
            {
                // Проверяем валидность сессии в Redis
                var userId = await sessionService.GetUserIdFromSessionAsync(sessionId);
                
                if (!string.IsNullOrEmpty(userId))
                {
                    // Сохраняем userId в HttpContext для доступа в контроллерах
                    context.Items["UserId"] = userId;
                    context.Items["SessionId"] = sessionId;
                    
                    // Продлеваем сессию при каждом запросе (rolling expiration)
                    await sessionService.ExtendSessionAsync(sessionId, TimeSpan.FromHours(24));
                    
                    _logger.LogDebug("Session validated for user {UserId}", userId);
                }
                else
                {
                    // Сессия истекла или невалидна - удаляем cookie
                    DeleteSessionCookie(context);
                    _logger.LogDebug("Session expired or invalid, cookie removed");
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

    /// <summary>
    /// Установить session cookie
    /// </summary>
    public static void SetSessionCookie(HttpContext context, string sessionId, TimeSpan expiration)
    {
        var isHttps = context.Request.IsHttps;
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.Add(expiration),
            Path = "/"
        };

        context.Response.Cookies.Append(SessionCookieName, sessionId, cookieOptions);
        
        var logger = context.RequestServices.GetService(typeof(ILogger<SessionCookieMiddleware>)) as ILogger<SessionCookieMiddleware>;
        logger?.LogInformation("Session cookie set: {SessionId} (HTTPS: {IsHttps})", sessionId, isHttps);
    }

    /// <summary>
    /// Удалить session cookie
    /// </summary>
    public static void DeleteSessionCookie(HttpContext context)
    {
        var isHttps = context.Request.IsHttps;
        
        context.Response.Cookies.Delete(SessionCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
    }
}
