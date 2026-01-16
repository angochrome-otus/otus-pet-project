using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Services;
using System.Text.Json;

namespace SteelDesignerEngineer.Infrastructure.Services;

/// <summary>
/// Реализация кэширования аутентификации через Redis
/// Infrastructure Layer
/// Redis используется только для кэша! MongoDB - источник данных!
/// SESSION-BASED AUTHENTICATION ONLY - NO JWT/REFRESH TOKENS
/// </summary>
public class AuthCacheService : IAuthCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<AuthCacheService> _logger;

    // Префиксы для Redis ключей
    private const string UserPrefix = "auth:user:";
    private const string LoginAttemptsPrefix = "auth:login_attempts:";

    public AuthCacheService(ICacheService cacheService, ILogger<AuthCacheService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    #region User Cache

    public async Task CacheUserAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{UserPrefix}{user.Id}";
            var json = JsonSerializer.Serialize(user);
            var expiration = TimeSpan.FromMinutes(15); // 15 минут кэш
            
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
