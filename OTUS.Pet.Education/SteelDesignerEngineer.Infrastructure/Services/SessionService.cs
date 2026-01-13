using Microsoft.Extensions.Logging;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;
using System.Security.Cryptography;

namespace SteelDesignerEngineer.Infrastructure.Services;

/// <summary>
/// Session management service implementation using Redis
/// Clean Architecture: Infrastructure Layer
/// </summary>
public class SessionService : ISessionService
{
    private readonly IRedisConnection _redisConnection;
    private readonly ISessionHistoryRepository _sessionHistoryRepository;
    private readonly ILogger<SessionService> _logger;
    private const string SessionKeyPrefix = "session:";

    public SessionService(
        IRedisConnection redisConnection,
        ISessionHistoryRepository sessionHistoryRepository,
        ILogger<SessionService> logger)
    {
        _redisConnection = redisConnection;
        _sessionHistoryRepository = sessionHistoryRepository;
        _logger = logger;
    }

    public async Task<string> CreateSessionAsync(
        string userId, 
        TimeSpan expiration, 
        string userName,
        string userEmail,
        string userRole,
        string? ipAddress = null, 
        string? userAgent = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate secure session ID
            var sessionId = GenerateSessionId();
            var key = GetSessionKey(sessionId);

            // Save to Redis
            await _redisConnection.SetAsync(key, userId, expiration);

            // Save to MongoDB history with user information
            var sessionHistory = new SessionHistory
            {
                UserId = userId,
                UserName = userName,
                UserEmail = userEmail,
                UserRole = userRole,
                SessionId = sessionId,
                LoginAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsActive = true
            };

            await _sessionHistoryRepository.CreateAsync(sessionHistory, cancellationToken);

            _logger.LogInformation("Session created for user {UserId} ({UserName}), expires in {Expiration}", 
                userId, userName, expiration);
            return sessionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user {UserId}", userId);
            throw;
        }
    }

    public async Task<string?> GetUserIdFromSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetSessionKey(sessionId);
            var userId = await _redisConnection.GetAsync(key);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogDebug("Session not found or expired: {SessionId}", sessionId);
                
                // Mark session as expired in MongoDB if it exists
                var sessionHistory = await _sessionHistoryRepository.GetBySessionIdAsync(sessionId, cancellationToken);
                if (sessionHistory != null && sessionHistory.IsActive)
                {
                    await _sessionHistoryRepository.UpdateLogoutAsync(
                        sessionId, 
                        DateTime.UtcNow, 
                        "expired", 
                        cancellationToken
                    );
                }
                
                return null;
            }

            _logger.LogDebug("Session found for user {UserId}", userId);
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user from session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetSessionKey(sessionId);
            await _redisConnection.RemoveAsync(key);

            // Update MongoDB history
            await _sessionHistoryRepository.UpdateLogoutAsync(
                sessionId, 
                DateTime.UtcNow, 
                "manual", 
                cancellationToken
            );

            _logger.LogInformation("Session deleted: {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task ExtendSessionAsync(string sessionId, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetSessionKey(sessionId);
            var userId = await _redisConnection.GetAsync(key);
            
            if (!string.IsNullOrEmpty(userId))
            {
                await _redisConnection.SetAsync(key, userId, expiration);
                _logger.LogDebug("Session extended: {SessionId}", sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending session {SessionId}", sessionId);
            throw;
        }
    }

    private static string GenerateSessionId()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private static string GetSessionKey(string sessionId)
    {
        return $"{SessionKeyPrefix}{sessionId}";
    }
}
