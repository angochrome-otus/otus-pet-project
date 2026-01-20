using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.ApiGateway.Session;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Pages;

public class TeacherDashboardModel : PageModel
{
    private readonly IAuthServiceClient _authServiceClient;
    private readonly ISessionServiceClient _sessionServiceClient;

    private static readonly TimeSpan SessionExpiration = TimeSpan.FromHours(24);

    public TeacherDashboardModel(IAuthServiceClient authServiceClient, ISessionServiceClient sessionServiceClient)
    {
        _authServiceClient = authServiceClient;
        _sessionServiceClient = sessionServiceClient;
    }

    public string? ErrorMessage { get; set; }

    public string UserId { get; private set; } = "-";
    public string Email { get; private set; } = "-";
    public string FirstName { get; private set; } = "-";
    public string LastName { get; private set; } = "-";
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string AvatarLetter => string.IsNullOrWhiteSpace(FirstName) ? "T" : FirstName[..1].ToUpperInvariant();

    public int TeachingCoursesCount { get; private set; }
    public int TotalStudents { get; private set; }
    public string AverageRating { get; private set; } = "N/A";
    public string TeachingSince { get; private set; } = "-";

    public string Specialization { get; private set; } = "Not set";
    public string AcademicTitle { get; private set; } = "Not set";
    public string Bio { get; private set; } = "Not set";

    public List<CourseItem> CourseItems { get; private set; } = new();

    public record CourseItem(string Title, string Meta);

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

        if (!string.Equals(res.Role, "Teacher", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(res.Role switch
            {
                "Student" => "/student-dashboard",
                "Admin" => "/admin-dashboard",
                _ => "/profile"
            });
        }

        UserId = res.UserId ?? "-";
        Email = res.Email ?? "-";
        FirstName = res.FirstName ?? "-";
        LastName = res.LastName ?? "-";

        // Without JS and without a rich GetUser payload, keep basic placeholders
        TeachingCoursesCount = 0;
        TotalStudents = 0;
        AverageRating = "N/A";
        TeachingSince = res.CreatedAt?.ToString("yyyy-MM") ?? "-";

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
