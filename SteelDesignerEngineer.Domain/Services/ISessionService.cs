namespace SteelDesignerEngineer.Domain.Services;

/// <summary>
/// Session management service interface
/// Clean Architecture: Domain Layer
/// Manages sessions in Redis and tracks history in MongoDB
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Create a new session in Redis and MongoDB history
    /// </summary>
    Task<string> CreateSessionAsync(
        string userId, 
        TimeSpan expiration, 
        string userName,
        string userEmail,
        string userRole,
        string? ipAddress = null, 
        string? userAgent = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user ID from session (Redis lookup)
    /// </summary>
    Task<string?> GetUserIdFromSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete session from Redis and update MongoDB history
    /// </summary>
    Task DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extend session expiration in Redis
    /// </summary>
    Task ExtendSessionAsync(string sessionId, TimeSpan expiration, CancellationToken cancellationToken = default);
}
