using Microsoft.AspNetCore.Mvc;
using SteelDesignerEngineer.Application.Interfaces;
using SteelDesignerEngineer.Domain.DTOs;
using SteelDesignerEngineer.Domain.Services;
using SteelDesignerEngineer.Infrastructure.Middleware;
using SteelDesignerEngineer.Infrastructure.Extensions;
using System.Security.Cryptography;

namespace SteelDesignerEngineer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthApplicationService _authService;
    private readonly ISessionService _sessionService;
    private readonly IOAuthService _oauthService;
    private readonly ILogger<AuthController> _logger;

    private static readonly TimeSpan SessionExpiration = TimeSpan.FromHours(24);

    public AuthController(
        IAuthApplicationService authService,
        ISessionService sessionService,
        IOAuthService oauthService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _sessionService = sessionService;
        _oauthService = oauthService;
        _logger = logger;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var response = await _authService.RegisterAsync(request, ipAddress, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        // Create session and set cookie with tracking
        if (response.User != null)
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            
            var sessionId = await _sessionService.CreateSessionAsync(
                response.User.Id, 
                SessionExpiration,
                response.User.FullName,
                response.User.Email,
                response.User.Role,
                ipAddress, 
                userAgent, 
                cancellationToken
            );
            
            SessionCookieMiddleware.SetSessionCookie(HttpContext, sessionId, SessionExpiration);
        }

        return Ok(response);
    }

    /// <summary>
    /// Авторизация пользователя
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var response = await _authService.LoginAsync(request, ipAddress, cancellationToken);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        // Create session and set cookie with tracking
        if (response.User != null)
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            
            var sessionId = await _sessionService.CreateSessionAsync(
                response.User.Id, 
                SessionExpiration,
                response.User.FullName,
                response.User.Email,
                response.User.Role,
                ipAddress, 
                userAgent, 
                cancellationToken
            );
            
            SessionCookieMiddleware.SetSessionCookie(HttpContext, sessionId, SessionExpiration);
        }

        _logger.LogInformation("User logged in: {Email}", request.Email);
        return Ok(response);
    }

    /// <summary>
    /// Get OAuth authorization URL
    /// </summary>
    [HttpGet("oauth/{provider}/url")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    public IActionResult GetOAuthUrl(string provider)
    {
        if (!_oauthService.IsProviderSupported(provider))
        {
            return BadRequest(new { message = $"Unsupported OAuth provider: {provider}" });
        }

        var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        HttpContext.Session.SetString($"oauth_state_{provider}", state);

        var authUrl = _oauthService.GetAuthorizationUrl(provider, state);

        if (string.IsNullOrEmpty(authUrl))
        {
            return BadRequest(new { message = $"{provider} OAuth not configured" });
        }

        return Ok(new { authUrl, state });
    }

    /// <summary>
    /// OAuth callback handler
    /// </summary>
    [HttpPost("oauth/callback")]
    [ProducesResponseType(typeof(OAuthLoginResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> OAuthCallback([FromBody] OAuthCallbackRequest request, CancellationToken cancellationToken)
    {
        if (!_oauthService.IsProviderSupported(request.Provider))
        {
            return BadRequest(new { message = $"Unsupported OAuth provider: {request.Provider}" });
        }

        var sessionState = HttpContext.Session.GetString($"oauth_state_{request.Provider}");
        if (!string.IsNullOrEmpty(sessionState) && sessionState != request.State)
        {
            _logger.LogWarning("OAuth CSRF state mismatch for provider: {Provider}", request.Provider);
            return BadRequest(new { message = "Invalid state parameter" });
        }

        var oauthUser = await _oauthService.GetUserInfoAsync(request.Provider, request.Code, cancellationToken);

        if (oauthUser == null || string.IsNullOrEmpty(oauthUser.Email))
        {
            _logger.LogError("Failed to get user info from {Provider}", request.Provider);
            return BadRequest(new { message = $"Failed to authenticate with {request.Provider}" });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var response = await _authService.OAuthLoginAsync(oauthUser, ipAddress, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        // Create session and set cookie with tracking
        if (response.User != null)
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            
            var sessionId = await _sessionService.CreateSessionAsync(
                response.User.Id, 
                SessionExpiration,
                response.User.FullName,
                response.User.Email,
                response.User.Role,
                ipAddress, 
                userAgent, 
                cancellationToken
            );
            
            SessionCookieMiddleware.SetSessionCookie(HttpContext, sessionId, SessionExpiration);
        }

        _logger.LogInformation("OAuth login successful: {Email} via {Provider}", oauthUser.Email, request.Provider);
        return Ok(response);
    }

    /// <summary>
    /// Выход из системы
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var sessionId = HttpContext.GetSessionId();
        if (!string.IsNullOrEmpty(sessionId))
        {
            await _sessionService.DeleteSessionAsync(sessionId, cancellationToken);
        }

        SessionCookieMiddleware.DeleteSessionCookie(HttpContext);

        _logger.LogInformation("User logged out");
        return Ok(new { message = "Successfully logged out" });
    }

    /// <summary>
    /// Получить текущего пользователя
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
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

        var user = await _authService.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Обновить профиль
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
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

        var user = await _authService.UpdateProfileAsync(userId, request, cancellationToken);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Изменить пароль
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
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

        var success = await _authService.ChangePasswordAsync(userId, request, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message = "Failed to change password" });
        }

        return Ok(new { message = "Password changed successfully" });
    }
}
