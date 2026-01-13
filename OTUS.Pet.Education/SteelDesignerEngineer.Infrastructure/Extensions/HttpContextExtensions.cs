using Microsoft.AspNetCore.Http;

namespace SteelDesignerEngineer.Infrastructure.Extensions;

/// <summary>
/// Extension methods дл€ работы с HttpContext и сесси€ми
/// Clean Architecture: Infrastructure Layer
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// ѕолучить UserId из текущей сессии
    /// </summary>
    public static string? GetUserIdFromSession(this HttpContext context)
    {
        return context.Items["UserId"] as string;
    }

    /// <summary>
    /// ѕолучить SessionId из текущей сессии
    /// </summary>
    public static string? GetSessionId(this HttpContext context)
    {
        return context.Items["SessionId"] as string;
    }

    /// <summary>
    /// ѕроверить, авторизован ли пользователь через сессию
    /// </summary>
    public static bool IsSessionAuthenticated(this HttpContext context)
    {
        return !string.IsNullOrEmpty(context.GetUserIdFromSession());
    }
}
