using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.Domain.DTOs;
using SteelDesignerEngineer.Domain.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SteelDesignerEngineer.Infrastructure.Services;

/// <summary>
/// OAuth authentication service implementation
/// Clean Architecture: Infrastructure Layer
/// Supports Google and GitHub OAuth 2.0
/// </summary>
public class OAuthService : IOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OAuthService> _logger;

    // OAuth endpoints
    private const string GoogleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string GoogleTokenUrl = "https://oauth2.googleapis.com/token";
    private const string GoogleUserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

    private const string GitHubAuthUrl = "https://github.com/login/oauth/authorize";
    private const string GitHubTokenUrl = "https://github.com/login/oauth/access_token";
    private const string GitHubUserInfoUrl = "https://api.github.com/user";
    private const string GitHubUserEmailUrl = "https://api.github.com/user/emails";

    public OAuthService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OAuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string provider, string state)
    {
        return provider.ToLower() switch
        {
            "google" => GetGoogleAuthUrl(state),
            "github" => GetGitHubAuthUrl(state),
            _ => throw new ArgumentException($"Unsupported provider: {provider}")
        };
    }

    public async Task<OAuthUserInfo?> GetUserInfoAsync(string provider, string code, CancellationToken cancellationToken = default)
    {
        return provider.ToLower() switch
        {
            "google" => await GetGoogleUserInfoAsync(code, cancellationToken),
            "github" => await GetGitHubUserInfoAsync(code, cancellationToken),
            _ => throw new ArgumentException($"Unsupported provider: {provider}")
        };
    }

    public bool IsProviderSupported(string provider)
    {
        return provider.ToLower() is "google" or "github";
    }

    #region Google OAuth

    private string GetGoogleAuthUrl(string state)
    {
        var clientId = _configuration["OAuth:Google:ClientId"];
        var redirectUri = _configuration["OAuth:Google:RedirectUri"];
        var scope = "openid profile email";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
        {
            _logger.LogWarning("Google OAuth not configured");
            return string.Empty;
        }

        return $"{GoogleAuthUrl}?" +
               $"client_id={Uri.EscapeDataString(clientId)}&" +
               $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
               $"response_type=code&" +
               $"scope={Uri.EscapeDataString(scope)}&" +
               $"state={Uri.EscapeDataString(state)}";
    }

    private async Task<OAuthUserInfo?> GetGoogleUserInfoAsync(string code, CancellationToken cancellationToken)
    {
        try
        {
            var clientId = _configuration["OAuth:Google:ClientId"];
            var clientSecret = _configuration["OAuth:Google:ClientSecret"];
            var redirectUri = _configuration["OAuth:Google:RedirectUri"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Google OAuth credentials not configured");
                return null;
            }

            // 1. Exchange code for access token
            var tokenRequest = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri! },
                { "grant_type", "authorization_code" }
            };

            var tokenResponse = await _httpClient.PostAsync(
                GoogleTokenUrl,
                new FormUrlEncodedContent(tokenRequest),
                cancellationToken
            );

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var error = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Google token exchange failed: {Error}", error);
                return null;
            }

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var accessToken = tokenData.GetProperty("access_token").GetString();

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Google access token is empty");
                return null;
            }

            // 2. Get user info
            var request = new HttpRequestMessage(HttpMethod.Get, GoogleUserInfoUrl);
            request.Headers.Authorization = new("Bearer", accessToken);
            var userInfoResponse = await _httpClient.SendAsync(request, cancellationToken);

            if (!userInfoResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Google user info request failed");
                return null;
            }

            var userInfo = await userInfoResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

            return new OAuthUserInfo(
                ProviderId: userInfo.GetProperty("id").GetString() ?? "",
                Email: userInfo.GetProperty("email").GetString() ?? "",
                FirstName: userInfo.TryGetProperty("given_name", out var firstName) ? firstName.GetString() ?? "" : "",
                LastName: userInfo.TryGetProperty("family_name", out var lastName) ? lastName.GetString() ?? "" : "",
                AvatarUrl: userInfo.TryGetProperty("picture", out var picture) ? picture.GetString() : null,
                Provider: "google"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google user info");
            return null;
        }
    }

    #endregion

    #region GitHub OAuth

    private string GetGitHubAuthUrl(string state)
    {
        var clientId = _configuration["OAuth:GitHub:ClientId"];
        var redirectUri = _configuration["OAuth:GitHub:RedirectUri"];
        var scope = "user:email";

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
        {
            _logger.LogWarning("GitHub OAuth not configured");
            return string.Empty;
        }

        return $"{GitHubAuthUrl}?" +
               $"client_id={Uri.EscapeDataString(clientId)}&" +
               $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
               $"scope={Uri.EscapeDataString(scope)}&" +
               $"state={Uri.EscapeDataString(state)}";
    }

    private async Task<OAuthUserInfo?> GetGitHubUserInfoAsync(string code, CancellationToken cancellationToken)
    {
        try
        {
            var clientId = _configuration["OAuth:GitHub:ClientId"];
            var clientSecret = _configuration["OAuth:GitHub:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("GitHub OAuth credentials not configured");
                return null;
            }

            // 1. Exchange code for access token
            var tokenRequest = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, GitHubTokenUrl)
            {
                Content = new FormUrlEncodedContent(tokenRequest)
            };
            request.Headers.Accept.Add(new("application/json"));

            var tokenResponse = await _httpClient.SendAsync(request, cancellationToken);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var error = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("GitHub token exchange failed: {Error}", error);
                return null;
            }

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var accessToken = tokenData.GetProperty("access_token").GetString();

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("GitHub access token is empty");
                return null;
            }

            // 2. Get user info
            var userRequest = new HttpRequestMessage(HttpMethod.Get, GitHubUserInfoUrl);
            userRequest.Headers.Authorization = new("Bearer", accessToken);
            userRequest.Headers.UserAgent.ParseAdd("SteelDesignerEngineer/1.0");

            var userInfoResponse = await _httpClient.SendAsync(userRequest, cancellationToken);

            if (!userInfoResponse.IsSuccessStatusCode)
            {
                _logger.LogError("GitHub user info request failed");
                return null;
            }

            var userInfo = await userInfoResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

            // Get email (might be in separate endpoint)
            var email = userInfo.TryGetProperty("email", out var emailProp) && !string.IsNullOrEmpty(emailProp.GetString())
                ? emailProp.GetString()
                : await GetGitHubPrimaryEmailAsync(accessToken, cancellationToken);

            // Parse name
            var name = userInfo.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
            var nameParts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            return new OAuthUserInfo(
                ProviderId: userInfo.GetProperty("id").GetInt32().ToString(),
                Email: email ?? "",
                FirstName: nameParts.Length > 0 ? nameParts[0] : "",
                LastName: nameParts.Length > 1 ? nameParts[1] : "",
                AvatarUrl: userInfo.TryGetProperty("avatar_url", out var avatar) ? avatar.GetString() : null,
                Provider: "github"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitHub user info");
            return null;
        }
    }

    private async Task<string?> GetGitHubPrimaryEmailAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, GitHubUserEmailUrl);
            request.Headers.Authorization = new("Bearer", accessToken);
            request.Headers.UserAgent.ParseAdd("SteelDesignerEngineer/1.0");

            var emailResponse = await _httpClient.SendAsync(request, cancellationToken);
            if (!emailResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var emails = await emailResponse.Content.ReadFromJsonAsync<JsonElement[]>(cancellationToken: cancellationToken);
            if (emails == null || emails.Length == 0)
            {
                return null;
            }

            // Find primary email
            foreach (var email in emails)
            {
                if (email.TryGetProperty("primary", out var isPrimary) && isPrimary.GetBoolean())
                {
                    return email.GetProperty("email").GetString();
                }
            }

            // Fallback to first email
            return emails[0].GetProperty("email").GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GitHub email");
            return null;
        }
    }

    #endregion
}
