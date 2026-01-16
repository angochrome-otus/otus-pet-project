using Microsoft.AspNetCore.Http;

namespace SteelDesignerEngineer.ApiGateway.Session;

public static class HttpContextSessionExtensions
{
    public static bool IsSessionAuthenticated(this HttpContext context)
        => context.Items.ContainsKey("UserId") && context.Items["UserId"] is string s && !string.IsNullOrWhiteSpace(s);

    public static string? GetUserIdFromSession(this HttpContext context)
        => context.Items.TryGetValue("UserId", out var v) ? v as string : null;

    public static string? GetSessionId(this HttpContext context)
        => context.Items.TryGetValue("SessionId", out var v) ? v as string : null;
}
