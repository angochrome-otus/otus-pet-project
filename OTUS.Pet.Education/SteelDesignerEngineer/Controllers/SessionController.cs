using Microsoft.AspNetCore.Mvc;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Infrastructure.Extensions;

namespace SteelDesignerEngineer.Controllers;

/// <summary>
/// API для работы с историей сессий
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionHistoryRepository _sessionHistoryRepository;
    private readonly ILogger<SessionController> _logger;

    public SessionController(
        ISessionHistoryRepository sessionHistoryRepository,
        ILogger<SessionController> logger)
    {
        _sessionHistoryRepository = sessionHistoryRepository;
        _logger = logger;
    }

    /// <summary>
    /// Получить активные сессии текущего пользователя
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetActiveSessions(CancellationToken cancellationToken)
    {
        if (!HttpContext.IsSessionAuthenticated())
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var userId = HttpContext.GetUserIdFromSession();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Session expired" });
        }

        var sessions = await _sessionHistoryRepository.GetActiveSessionsByUserIdAsync(userId, cancellationToken);

        return Ok(new
        {
            activeSessions = sessions.Count,
            sessions = sessions.Select(s => new
            {
                sessionId = s.SessionId,
                loginAt = s.LoginAt,
                ipAddress = s.IpAddress,
                userAgent = s.UserAgent,
                isCurrent = s.SessionId == HttpContext.GetSessionId()
            })
        });
    }

    /// <summary>
    /// Получить историю сессий текущего пользователя
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetSessionHistory(
        [FromQuery] int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        if (!HttpContext.IsSessionAuthenticated())
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var userId = HttpContext.GetUserIdFromSession();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Session expired" });
        }

        var sessions = await _sessionHistoryRepository.GetSessionHistoryByUserIdAsync(userId, limit, cancellationToken);

        return Ok(new
        {
            totalSessions = sessions.Count,
            sessions = sessions.Select(s => new
            {
                sessionId = s.SessionId,
                loginAt = s.LoginAt,
                logoutAt = s.LogoutAt,
                ipAddress = s.IpAddress,
                userAgent = s.UserAgent,
                logoutType = s.LogoutType,
                durationSeconds = s.DurationSeconds,
                isActive = s.IsActive
            })
        });
    }

    /// <summary>
    /// Завершить все сессии кроме текущей (logout from other devices)
    /// </summary>
    [HttpPost("logout-others")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> LogoutOtherSessions(CancellationToken cancellationToken)
    {
        if (!HttpContext.IsSessionAuthenticated())
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var userId = HttpContext.GetUserIdFromSession();
        var currentSessionId = HttpContext.GetSessionId();
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(currentSessionId))
        {
            return Unauthorized(new { message = "Session expired" });
        }

        var activeSessions = await _sessionHistoryRepository.GetActiveSessionsByUserIdAsync(userId, cancellationToken);
        var logoutAt = DateTime.UtcNow;

        foreach (var session in activeSessions.Where(s => s.SessionId != currentSessionId))
        {
            await _sessionHistoryRepository.UpdateLogoutAsync(
                session.SessionId, 
                logoutAt, 
                "revoked", 
                cancellationToken
            );
        }

        _logger.LogInformation("User {UserId} logged out from {Count} other devices", userId, activeSessions.Count - 1);

        return Ok(new { message = $"Logged out from {activeSessions.Count - 1} other devices" });
    }
}
