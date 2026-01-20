using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.ApiGateway.Session;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Pages;

public class StudentDashboardModel : PageModel
{
    private readonly IAuthServiceClient _authServiceClient;
    private readonly ISessionServiceClient _sessionServiceClient;

    public StudentDashboardModel(IAuthServiceClient authServiceClient, ISessionServiceClient sessionServiceClient)
    {
        _authServiceClient = authServiceClient;
        _sessionServiceClient = sessionServiceClient;
    }

    public string? ErrorMessage { get; set; }

    public string Email { get; private set; } = "-";
    public string FirstName { get; private set; } = "-";
    public string LastName { get; private set; } = "-";
    public string FullName => $"{FirstName} {LastName}".Trim();

    public int EnrolledCoursesCount { get; private set; }
    public int CompletedCoursesCount { get; private set; }

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
            ErrorMessage = "Authentication service is temporarily unavailable.";
            return Page();
        }

        if (!res.Success || string.IsNullOrWhiteSpace(res.UserId))
        {
            return Redirect("/login");
        }

        if (!string.Equals(res.Role, "Student", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(res.Role switch
            {
                "Teacher" => "/teacher-dashboard",
                "Admin" => "/admin-dashboard",
                _ => "/profile"
            });
        }

        Email = res.Email ?? "-";
        FirstName = res.FirstName ?? "-";
        LastName = res.LastName ?? "-";

        EnrolledCoursesCount = 0;
        CompletedCoursesCount = 0;

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
