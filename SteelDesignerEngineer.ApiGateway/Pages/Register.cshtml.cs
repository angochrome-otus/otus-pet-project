using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.ApiGateway.Session;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Pages;

public class RegisterModel : PageModel
{
    private readonly IAuthServiceClient _authServiceClient;
    private readonly ISessionServiceClient _sessionServiceClient;

    private static readonly TimeSpan SessionExpiration = TimeSpan.FromHours(24);

    public RegisterModel(IAuthServiceClient authServiceClient, ISessionServiceClient sessionServiceClient)
    {
        _authServiceClient = authServiceClient;
        _sessionServiceClient = sessionServiceClient;
    }

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Role { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public IActionResult OnGet()
    {
        if (HttpContext.IsSessionAuthenticated())
        {
            return Redirect("/profile");
        }

        // Ensure fields are empty on GET (no default admin/demo values)
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Role = string.Empty;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "? Please fill in first and last name";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "? Please provide email and password";
            return Page();
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "? Password must be at least 6 characters";
            return Page();
        }

        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            ErrorMessage = "? Passwords do not match";
            return Page();
        }

        if (Role != "Student" && Role != "Teacher")
        {
            ErrorMessage = "? Please choose a role";
            return Page();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var registerRes = await _authServiceClient.RegisterAsync(new RegisterUserRequest
        {
            Email = Email,
            Password = Password,
            FirstName = FirstName,
            LastName = LastName,
            Role = Role,
            IpAddress = ipAddress
        }, cancellationToken);

        if (registerRes == null)
        {
            ErrorMessage = "? Auth service unavailable";
            return Page();
        }

        if (!registerRes.Success)
        {
            var msg = registerRes.Message ?? "Registration error. User may already exist.";
            ErrorMessage = $"? {msg}";
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(registerRes.UserId))
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var sessionRes = await _sessionServiceClient.CreateSessionAsync(new CreateSessionRequest
            {
                UserId = registerRes.UserId,
                UserName = $"{registerRes.FirstName} {registerRes.LastName}".Trim(),
                UserEmail = registerRes.Email ?? string.Empty,
                UserRole = registerRes.Role ?? string.Empty,
                IpAddress = ipAddress,
                UserAgent = userAgent
            }, cancellationToken);

            if (sessionRes != null && sessionRes.Success && !string.IsNullOrWhiteSpace(sessionRes.SessionId))
            {
                SessionCookieMiddleware.SetSessionCookie(HttpContext, sessionRes.SessionId, SessionExpiration);
            }
        }

        return Redirect("/profile");
    }
}
