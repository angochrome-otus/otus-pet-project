using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SteelDesignerEngineer.ApiGateway.Clients;
using SteelDesignerEngineer.ApiGateway.Session;
using SteelDesignerEngineer.Contracts.Messages;

namespace SteelDesignerEngineer.ApiGateway.Pages;

public sealed class AdminSearchModel : PageModel
{
    private readonly ISearchServiceClient _search;
    private readonly ISessionServiceClient _session;

    public AdminSearchModel(ISearchServiceClient search, ISessionServiceClient session)
    {
        _search = search;
        _session = session;
    }

    public string? ErrorMessage { get; private set; }

    [BindProperty]
    public string CollectionName { get; set; } = "MyCollection";

    [BindProperty]
    public string? PointId { get; set; }

    [BindProperty]
    public string Text { get; set; } = string.Empty;

    public string? ResultJson { get; private set; }

    public IActionResult OnGet()
    {
        if (!HttpContext.IsSessionAuthenticated())
            return Redirect("/login");

        var role = (HttpContext.Items.TryGetValue("UserRole", out var r) ? r as string : null) ?? string.Empty;
        if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            return Redirect("/profile");

        return Page();
    }

    public async Task<IActionResult> OnPostUpsertAsync(CancellationToken cancellationToken)
    {
        if (!HttpContext.IsSessionAuthenticated())
            return Redirect("/login");

        var role = (HttpContext.Items.TryGetValue("UserRole", out var r) ? r as string : null) ?? string.Empty;
        if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            return Redirect("/profile");

        if (string.IsNullOrWhiteSpace(Text))
        {
            ErrorMessage = "Text is empty";
            return Page();
        }

        var res = await _search.UpsertTextEmbeddingAsync(new UpsertTextEmbeddingRequest
        {
            Text = Text,
            CollectionName = string.IsNullOrWhiteSpace(CollectionName) ? null : CollectionName,
            PointId = string.IsNullOrWhiteSpace(PointId) ? null : PointId,
            Payload = new Dictionary<string, object?>
            {
                ["title"] = "Admin test",
                ["pageName"] = "",
                ["snippet"] = Text.Length > 240 ? Text[..240] : Text
            }
        }, cancellationToken);

        ResultJson = JsonSerializer.Serialize(res, new JsonSerializerOptions { WriteIndented = true });

        if (res == null || !res.Success)
        {
            ErrorMessage = res?.Message ?? "Search service unavailable";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostLogoutAsync(CancellationToken cancellationToken)
    {
        var sessionId = HttpContext.GetSessionId();
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            await _session.DeleteSessionAsync(new DeleteSessionRequest { SessionId = sessionId }, cancellationToken);
        }

        SessionCookieMiddleware.DeleteSessionCookie(HttpContext);
        return Redirect("/login");
    }
}
