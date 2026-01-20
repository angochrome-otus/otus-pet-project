using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.ApiGateway.Session;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthServiceClient _authServiceClient;
    private readonly ISessionServiceClient _sessionServiceClient;

    private static readonly TimeSpan SessionExpiration = TimeSpan.FromHours(24);

    public LoginModel(IAuthServiceClient authServiceClient, ISessionServiceClient sessionServiceClient)
    {
        _authServiceClient = authServiceClient;
        _sessionServiceClient = sessionServiceClient;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public IActionResult OnGet()
    {
        // Always show login page when explicitly navigating to /login
        // Ensure fields are not prefilled
        Email = string.Empty;
        Password = string.Empty;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "? Please provide email and password";
            return Page();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var loginRes = await _authServiceClient.LoginAsync(new LoginUserRequest
        {
            Email = Email,
            Password = Password,
            IpAddress = ipAddress
        }, cancellationToken);

        if (loginRes == null)
        {
            ErrorMessage = "? Auth service unavailable";
            return Page();
        }

        if (!loginRes.Success)
        {
            var msg = loginRes.Message ?? "Invalid email or password";
            ErrorMessage = $"? {msg}";
            return Page();
        }

        var actualRole = (loginRes.Role ?? string.Empty).Trim();

        if (!string.IsNullOrWhiteSpace(loginRes.UserId))
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var sessionRes = await _sessionServiceClient.CreateSessionAsync(new CreateSessionRequest
            {
                UserId = loginRes.UserId,
                UserName = $"{loginRes.FirstName} {loginRes.LastName}".Trim(),
                UserEmail = loginRes.Email ?? string.Empty,
                UserRole = actualRole,
                IpAddress = ipAddress,
                UserAgent = userAgent
            }, cancellationToken);

            if (sessionRes != null && sessionRes.Success && !string.IsNullOrWhiteSpace(sessionRes.SessionId))
            {
                SessionCookieMiddleware.SetSessionCookie(HttpContext, sessionRes.SessionId, SessionExpiration);
            }
        }

        // Profile page will redirect based on role.
        return Redirect("/profile");
    }
}
