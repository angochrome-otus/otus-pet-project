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

        if (!string.IsNullOrWhiteSpace(res.UserId))
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var sessionRes = await _sessionService.CreateSessionAsync(new CreateSessionRequest
            {
                UserId = res.UserId,
                UserName = $"{res.FirstName} {res.LastName}".Trim(),
                UserEmail = res.Email ?? string.Empty,
                UserRole = res.Role ?? string.Empty,
                IpAddress = ipAddress,
                UserAgent = userAgent
            }, cancellationToken);

            if (sessionRes != null && sessionRes.Success && !string.IsNullOrWhiteSpace(sessionRes.SessionId))
                SessionCookieMiddleware.SetSessionCookie(HttpContext, sessionRes.SessionId, SessionExpiration);
        }

        // Return formatted response matching frontend expectations
        return Ok(new
        {
            Success = res.Success,
            Message = res.Message,
            User = new
            {
                Id = res.UserId,
                Email = res.Email,
                FirstName = res.FirstName,
                LastName = res.LastName,
                Role = res.Role,
                AvatarUrl = (string?)null  // Register doesn't return avatar
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

        if (!string.IsNullOrWhiteSpace(res.UserId))
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var sessionRes = await _sessionService.CreateSessionAsync(new CreateSessionRequest
            {
                UserId = res.UserId,
                UserName = $"{res.FirstName} {res.LastName}".Trim(),
                UserEmail = res.Email ?? string.Empty,
                UserRole = res.Role ?? string.Empty,
                IpAddress = ipAddress,
                UserAgent = userAgent
            }, cancellationToken);

            if (sessionRes != null && sessionRes.Success && !string.IsNullOrWhiteSpace(sessionRes.SessionId))
                SessionCookieMiddleware.SetSessionCookie(HttpContext, sessionRes.SessionId, SessionExpiration);
        }

        _logger.LogInformation("User logged in: {Email}", request.Email);

        // Return formatted response matching frontend expectations
        return Ok(new
        {
            Success = res.Success,
            Message = res.Message,
            User = new
            {
                Id = res.UserId,
                Email = res.Email,
                FirstName = res.FirstName,
                LastName = res.LastName,
                Role = res.Role,
                AvatarUrl = res.AvatarUrl
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
    /// Получить текущего пользователя
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

        return Ok(res);
    }

    /// <summary>
    /// Обновить профиль
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
    /// Изменить пароль
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
