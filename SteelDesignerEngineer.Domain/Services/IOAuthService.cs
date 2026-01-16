using SteelDesignerEngineer.Domain.DTOs;

namespace SteelDesignerEngineer.Domain.Services;

/// <summary>
/// OAuth authentication service interface
/// Clean Architecture: Domain Layer
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// Get OAuth authorization URL for user to visit
    /// </summary>
    string GetAuthorizationUrl(string provider, string state);
    
    /// <summary>
    /// Exchange authorization code for access token and get user info
    /// </summary>
    Task<OAuthUserInfo?> GetUserInfoAsync(string provider, string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Supported OAuth providers
    /// </summary>
    bool IsProviderSupported(string provider);
}
