using Microsoft.AspNetCore.Mvc;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.ApiGateway.Session;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServiceClient _authService;
    private readonly ISessionServiceClient _sessionService;
    private readonly ILogger<AuthController> _logger;

    private static readonly TimeSpan SessionExpiration = TimeSpan.FromHours(24);

    public AuthController(
        IAuthServiceClient authService,
        ISessionServiceClient sessionService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        request = request with { IpAddress = ipAddress };

        var res = await _authService.RegisterAsync(request, cancellationToken);
        if (res == null)
            return BadRequest(new { Success = false, Message = "Auth service unavailable" });
        if (!res.Success)
            return BadRequest(res);

        var role = (res.Role ?? string.Empty).ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(res.UserId))
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var sessionRes = await _sessionService.CreateSessionAsync(new CreateSessionRequest
            {
                UserId = res.UserId,
                UserName = $"{res.FirstName} {res.LastName}".Trim(),
                UserEmail = res.Email ?? string.Empty,
                UserRole = role,
                IpAddress = ipAddress,
                UserAgent = userAgent
            }, cancellationToken);

            if (sessionRes != null && sessionRes.Success && !string.IsNullOrWhiteSpace(sessionRes.SessionId))
                SessionCookieMiddleware.SetSessionCookie(HttpContext, sessionRes.SessionId, SessionExpiration);
        }

        // Return formatted response matching frontend expectations
        return Ok(new
        {
            success = res.Success,
            message = res.Message,
            user = new
            {
                id = res.UserId,
                email = res.Email,
                firstName = res.FirstName,
                lastName = res.LastName,
                role = role,
                avatarUrl = (string?)null  // Register doesn't return avatar
            }
        });
    }

    /// <summary>
    /// Авторизация пользователя
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        request = request with { IpAddress = ipAddress };

        var res = await _authService.LoginAsync(request, cancellationToken);
        if (res == null)
            return Unauthorized(new { Success = false, Message = "Auth service unavailable" });
        if (!res.Success)
            return Unauthorized(res);

        var role = (res.Role ?? string.Empty).ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(res.UserId))
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var sessionRes = await _sessionService.CreateSessionAsync(new CreateSessionRequest
            {
                UserId = res.UserId,
                UserName = $"{res.FirstName} {res.LastName}".Trim(),
                UserEmail = res.Email ?? string.Empty,
                UserRole = role,
                IpAddress = ipAddress,
                UserAgent = userAgent
            }, cancellationToken);

            if (sessionRes == null)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { success = false, message = "Session service unavailable" });

            if (!sessionRes.Success || string.IsNullOrWhiteSpace(sessionRes.SessionId))
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { success = false, message = "Failed to create session" });

            SessionCookieMiddleware.SetSessionCookie(HttpContext, sessionRes.SessionId, SessionExpiration);
        }

        _logger.LogInformation("User logged in: {Email} with role {Role}", request.Email, role);

        // Return formatted response matching frontend expectations
        return Ok(new
        {
            success = res.Success,
            message = res.Message,
            user = new
            {
                id = res.UserId,
                email = res.Email,
                firstName = res.FirstName,
                lastName = res.LastName,
                role = role,
                avatarUrl = res.AvatarUrl
            }
        });
    }

    /// <summary>
    /// Выход из системы
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var sessionId = HttpContext.GetSessionId();
        if (!string.IsNullOrWhiteSpace(sessionId))
            await _sessionService.DeleteSessionAsync(new DeleteSessionRequest { SessionId = sessionId }, cancellationToken);

        SessionCookieMiddleware.DeleteSessionCookie(HttpContext);
        return Ok(new { message = "Successfully logged out" });
    }

    /// <summary>
    /// Профиль текущего пользователя
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        if (!HttpContext.IsSessionAuthenticated())
            return Unauthorized(new { message = "Not authenticated" });

        var userId = HttpContext.GetUserIdFromSession();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "Session expired" });

        var res = await _authService.GetUserAsync(new GetUserRequest { UserId = userId }, cancellationToken);
        if (res == null || !res.Success)
            return NotFound();

        // Normalize role for frontend
        var role = (res.Role ?? string.Empty).ToLowerInvariant();

        return Ok(new
        {
            success = true,
            user = new
            {
                id = res.UserId,
                email = res.Email,
                firstName = res.FirstName,
                lastName = res.LastName,
                role = role,
                avatarUrl = res.AvatarUrl,
                createdAt = res.CreatedAt,
                lastLoginAt = res.LastLoginAt
            }
        });
    }

    /// <summary>
    /// Обновление профиля
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!HttpContext.IsSessionAuthenticated())
            return Unauthorized(new { message = "Not authenticated" });

        var userId = HttpContext.GetUserIdFromSession();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "Session expired" });

        request = request with { UserId = userId };
        var res = await _authService.UpdateProfileAsync(request, cancellationToken);

        if (res == null)
            return BadRequest(new { Success = false, Message = "Auth service unavailable" });
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    /// <summary>
    /// Смена пароля
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!HttpContext.IsSessionAuthenticated())
            return Unauthorized(new { message = "Not authenticated" });

        var userId = HttpContext.GetUserIdFromSession();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { message = "Session expired" });

        request = request with { UserId = userId };
        var res = await _authService.ChangePasswordAsync(request, cancellationToken);

        if (res == null)
            return BadRequest(new { Success = false, Message = "Auth service unavailable" });
        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }
}
