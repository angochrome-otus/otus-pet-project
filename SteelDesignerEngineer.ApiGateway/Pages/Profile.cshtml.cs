using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.ApiGateway.Session;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Pages;

public class ProfileModel : PageModel
{
    private readonly IAuthServiceClient _authServiceClient;
    private readonly ISessionServiceClient _sessionServiceClient;

    public ProfileModel(IAuthServiceClient authServiceClient, ISessionServiceClient sessionServiceClient)
    {
        _authServiceClient = authServiceClient;
        _sessionServiceClient = sessionServiceClient;
    }

    public string? ErrorMessage { get; set; }

    public string UserId { get; private set; } = "-";
    public string Email { get; private set; } = "-";
    public string FirstName { get; private set; } = "-";
    public string LastName { get; private set; } = "-";

    // Store role as normalized lowercase for comparisons
    public string Role { get; private set; } = "-";

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string RoleCss => Role switch
    {
        "student" => "role-student",
        "teacher" => "role-teacher",
        "admin" => "role-admin",
        _ => string.Empty
    };

    // Canonical dashboard URL after auth/registration.
    public string DashboardUrl => Role switch
    {
        "student" => "/student-dashboard",
        "teacher" => "/teacher-dashboard",
        "admin" => "/admin-dashboard",
        _ => "/"
    };

    public string ActiveCourses { get; private set; } = "-";
    public string CompletedCourses { get; private set; } = "-";
    public string MemberSince { get; private set; } = "-";
    public string LastLogin { get; private set; } = "-";

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!HttpContext.IsSessionAuthenticated())
        {
            return Redirect("/login");
        }

        var userId = HttpContext.GetUserIdFromSession();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Redirect("/login");
        }

        var res = await _authServiceClient.GetUserAsync(new GetUserRequest { UserId = userId }, cancellationToken);
        if (res == null)
        {
            ErrorMessage = "—ервис авторизации временно недоступен.";
            return Page();
        }

        if (!res.Success || string.IsNullOrWhiteSpace(res.UserId))
        {
            return Redirect("/login");
        }

        UserId = res.UserId ?? "-";
        Email = res.Email ?? "-";
        FirstName = res.FirstName ?? "-";
        LastName = res.LastName ?? "-";
        Role = (res.Role ?? string.Empty).ToLowerInvariant();

        ActiveCourses = "-";
        CompletedCourses = "-";
        MemberSince = res.CreatedAt?.ToString("yyyy-MM-dd") ?? "-";
        LastLogin = res.LastLoginAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";

        return Page();
    }

    public async Task<IActionResult> OnPostLogoutAsync(CancellationToken cancellationToken)
    {
        var sessionId = HttpContext.GetSessionId();
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            await _sessionServiceClient.DeleteSessionAsync(new DeleteSessionRequest { SessionId = sessionId }, cancellationToken);
        }

        SessionCookieMiddleware.DeleteSessionCookie(HttpContext);
        return Redirect("/login");
    }
}
