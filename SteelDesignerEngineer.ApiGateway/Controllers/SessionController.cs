using Microsoft.AspNetCore.Mvc;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.ApiGateway.Session;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Controllers;

/// <summary>
/// API для работы с сессиями пользователя
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionServiceClient _sessionService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionServiceClient sessionService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Получить историю сессий текущего пользователя
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetSessionHistory(
        [FromQuery] int limit = 50, 
        CancellationToken cancellationToken = default)
    {
        if (!HttpContext.IsSessionAuthenticated())
            return Unauthorized(new { message = "Not authenticated" });

        var userId = HttpContext.GetUserIdFromSession();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "Session expired" });

        var res = await _sessionService.GetUserSessionsAsync(new GetUserSessionsRequest { UserId = userId }, cancellationToken);
        if (res == null)
            return BadRequest(new { Success = false, Message = "Session service unavailable" });
        if (!res.Success)
            return BadRequest(res);

        var sessions = res.Sessions ?? new List<SessionInfo>();

        return Ok(new
        {
            totalSessions = sessions.Count,
            sessions = sessions.Take(limit).Select(s => new
            {
                sessionId = s.SessionId,
                loginAt = s.LoginAt,
                ipAddress = s.IpAddress,
                userAgent = s.UserAgent,
                isActive = s.IsActive
            })
        });
    }

    /// <summary>
    /// Получить активные сессии текущего пользователя
    /// </summary>
    [HttpGet("active")]
    public IActionResult GetActiveSessions(CancellationToken cancellationToken)
    {
        if (!HttpContext.IsSessionAuthenticated())
            return Unauthorized(new { message = "Not authenticated" });

        return Ok(new
        {
            activeSessions = 1,
            currentSessionId = HttpContext.GetSessionId()
        });
    }

    /// <summary>
    /// Завершить все сессии кроме текущей (logout from other devices)
    /// </summary>
    [HttpPost("logout-others")]
    public IActionResult LogoutOtherSessions(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logout-others requested but not supported");
        return StatusCode(501, new { message = "Not implemented" });
    }
}
