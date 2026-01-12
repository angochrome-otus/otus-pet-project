using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Services;
using System.Text.Json;

namespace SteelDesignerEngineer.Infrastructure.Services;

/// <summary>
/// Реализация кеширования авторизации через Redis
/// Infrastructure Layer
/// Redis используется ТОЛЬКО для кеша! MongoDB - источник истины!
/// </summary>
public class AuthCacheService : IAuthCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<AuthCacheService> _logger;

    // Префиксы для Redis ключей
    private const string RefreshTokenPrefix = "auth:refresh_token:";
    private const string UserPrefix = "auth:user:";
    private const string BlacklistPrefix = "auth:blacklist:";
    private const string LoginAttemptsPrefix = "auth:login_attempts:";

    public AuthCacheService(ICacheService cacheService, ILogger<AuthCacheService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    #region Refresh Token Cache

    public async Task CacheRefreshTokenAsync(string token, string userId, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{RefreshTokenPrefix}{token}";
            await _cacheService.SetAsync(key, userId, expiration, cancellationToken);
            _logger.LogDebug("Cached refresh token in Redis: {Key}, TTL: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache refresh token in Redis (non-critical): {Token}", token);
            // Не падаем если Redis недоступен - MongoDB работает!
        }
    }

    public async Task<string?> GetCachedUserIdByRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{RefreshTokenPrefix}{token}";
            var userId = await _cacheService.GetAsync(key, cancellationToken);
            
            if (userId != null)
            {
                _logger.LogDebug("Refresh token found in Redis cache: {Key}", key);
            }
            
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get refresh token from Redis cache (will check MongoDB)");
            return null; // Fallback к MongoDB
        }
    }

    public async Task RemoveCachedRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{RefreshTokenPrefix}{token}";
            await _cacheService.DeleteAsync(key, cancellationToken);
            _logger.LogDebug("Removed refresh token from Redis cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove refresh token from Redis cache (non-critical)");
        }
    }

    #endregion

    #region User Cache

    public async Task CacheUserAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{UserPrefix}{user.Id}";
            var json = JsonSerializer.Serialize(user);
            var expiration = TimeSpan.FromMinutes(15); // 15 минут кеш
            
            await _cacheService.SetAsync(key, json, expiration, cancellationToken);
            _logger.LogDebug("Cached user in Redis: {UserId}, TTL: {Expiration}", user.Id, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache user in Redis (non-critical): {UserId}", user.Id);
        }
    }

    public async Task<User?> GetCachedUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{UserPrefix}{userId}";
            var json = await _cacheService.GetAsync(key, cancellationToken);
            
            if (json == null)
            {
                _logger.LogDebug("User not found in Redis cache: {UserId}", userId);
                return null;
            }

            var user = JsonSerializer.Deserialize<User>(json);
            _logger.LogDebug("User found in Redis cache: {UserId}", userId);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get user from Redis cache (will check MongoDB): {UserId}", userId);
            return null; // Fallback к MongoDB
        }
    }

    public async Task InvalidateUserCacheAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{UserPrefix}{userId}";
            await _cacheService.DeleteAsync(key, cancellationToken);
            _logger.LogDebug("Invalidated user cache in Redis: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate user cache in Redis (non-critical): {UserId}", userId);
        }
    }

    #endregion

    #region JWT Blacklist

    public async Task BlacklistAccessTokenAsync(string token, TimeSpan remainingTime, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{BlacklistPrefix}{token}";
            await _cacheService.SetAsync(key, "blacklisted", remainingTime, cancellationToken);
            _logger.LogInformation("Added JWT token to blacklist in Redis, TTL: {RemainingTime}", remainingTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to blacklist JWT token in Redis (CRITICAL - logout may not work!)");
            // Это критично для logout! Но не падаем
        }
    }

    public async Task<bool> IsAccessTokenBlacklistedAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{BlacklistPrefix}{token}";
            var value = await _cacheService.GetAsync(key, cancellationToken);
            
            if (value != null)
            {
                _logger.LogWarning("JWT token is blacklisted: {Token}", token[..20] + "...");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check JWT blacklist in Redis (allowing access)");
            return false; // Если Redis недоступен - пропускаем
        }
    }

    #endregion

    #region Rate Limiting

    public async Task RecordLoginAttemptAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{LoginAttemptsPrefix}{email}";
            var currentCount = await GetLoginAttemptsCountAsync(email, cancellationToken);
            var newCount = currentCount + 1;
            var expiration = TimeSpan.FromMinutes(15);
            
            await _cacheService.SetAsync(key, newCount.ToString(), expiration, cancellationToken);
            _logger.LogDebug("Recorded login attempt for {Email}: {Count}", email, newCount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record login attempt in Redis (non-critical): {Email}", email);
        }
    }

    public async Task<int> GetLoginAttemptsCountAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{LoginAttemptsPrefix}{email}";
            var value = await _cacheService.GetAsync(key, cancellationToken);
            
            if (value == null)
            {
                return 0;
            }

            return int.TryParse(value, out var count) ? count : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get login attempts from Redis (allowing login): {Email}", email);
            return 0; // Если Redis недоступен - не блокируем
        }
    }

    public async Task ResetLoginAttemptsAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{LoginAttemptsPrefix}{email}";
            await _cacheService.DeleteAsync(key, cancellationToken);
            _logger.LogDebug("Reset login attempts for {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to reset login attempts in Redis (non-critical): {Email}", email);
        }
    }

    #endregion
}
