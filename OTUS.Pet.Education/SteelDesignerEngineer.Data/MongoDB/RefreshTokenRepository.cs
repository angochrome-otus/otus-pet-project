using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;

namespace SteelDesignerEngineer.Data.MongoDB;

/// <summary>
/// Репозиторий для Refresh Tokens в MongoDB
/// DATA LAYER - все операции с БД здесь!
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _refreshTokens;
    private readonly ILogger<RefreshTokenRepository> _logger;

    public RefreshTokenRepository(IMongoDatabase database, ILogger<RefreshTokenRepository> logger)
    {
        _refreshTokens = database.GetCollection<RefreshToken>("RefreshTokens");
        _logger = logger;
        
        // Создаем индексы для быстрого поиска
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        try
        {
            // Индекс на token (unique) для быстрого поиска
            var tokenIndex = Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.Token);
            var tokenOptions = new CreateIndexOptions { Unique = true };
            _refreshTokens.Indexes.CreateOne(new CreateIndexModel<RefreshToken>(tokenIndex, tokenOptions));

            // Индекс на userId для получения всех токенов пользователя
            var userIdIndex = Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.UserId);
            _refreshTokens.Indexes.CreateOne(new CreateIndexModel<RefreshToken>(userIdIndex));

            // TTL индекс на expiresAt для автоматического удаления истекших токенов
            var expiresAtIndex = Builders<RefreshToken>.IndexKeys.Ascending(rt => rt.ExpiresAt);
            var ttlOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
            _refreshTokens.Indexes.CreateOne(new CreateIndexModel<RefreshToken>(expiresAtIndex, ttlOptions));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create indexes on RefreshTokens collection");
        }
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            await _refreshTokens.InsertOneAsync(refreshToken, cancellationToken: cancellationToken);
            _logger.LogInformation("Refresh token created in MongoDB for user: {UserId}", refreshToken.UserId);
            return refreshToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating refresh token in MongoDB for user: {UserId}", refreshToken.UserId);
            throw;
        }
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _refreshTokens
                .Find(rt => rt.Token == token)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting refresh token from MongoDB: {Token}", token);
            throw;
        }
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _refreshTokens
                .Find(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active refresh tokens from MongoDB for user: {UserId}", userId);
            throw;
        }
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true)
                .Set(rt => rt.RevokedAt, DateTime.UtcNow);

            await _refreshTokens.UpdateOneAsync(
                rt => rt.Token == token,
                update,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Refresh token revoked in MongoDB: {Token}", token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token in MongoDB: {Token}", token);
            throw;
        }
    }

    public async Task RevokeAllByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true)
                .Set(rt => rt.RevokedAt, DateTime.UtcNow);

            await _refreshTokens.UpdateManyAsync(
                rt => rt.UserId == userId && !rt.IsRevoked,
                update,
                cancellationToken: cancellationToken);

            _logger.LogInformation("All refresh tokens revoked in MongoDB for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all refresh tokens in MongoDB for user: {UserId}", userId);
            throw;
        }
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _refreshTokens.DeleteManyAsync(
                rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked,
                cancellationToken);

            _logger.LogInformation("Deleted {Count} expired refresh tokens from MongoDB", result.DeletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expired refresh tokens from MongoDB");
            throw;
        }
    }
}
