using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;

namespace SteelDesignerEngineer.Data.MongoDB;

/// <summary>
/// Репозиторий для работы с пользователями в MongoDB
/// Clean Architecture: Data/Infrastructure Layer
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IMongoDatabase database, ILogger<UserRepository> logger)
    {
        _users = database.GetCollection<User>("Users");
        _logger = logger;
        
        // Создаем индексы
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Уникальный индекс по Email
        var emailIndexModel = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true }
        );

        // Индекс по AuthProvider + OAuthProviderId
        var oauthIndexModel = new CreateIndexModel<User>(
            Builders<User>.IndexKeys
                .Ascending(u => u.AuthProvider)
                .Ascending(u => u.OAuthProviderId),
            new CreateIndexOptions { Unique = false }
        );

        try
        {
            _users.Indexes.CreateMany(new[] { emailIndexModel, oauthIndexModel });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create indexes on User collection - they may already exist");
        }
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {Id}", id);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<User?> GetByOAuthProviderAsync(string provider, string providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _users.Find(u => u.AuthProvider == provider && u.OAuthProviderId == providerId)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by OAuth provider: {Provider}, Id: {ProviderId}", provider, providerId);
            throw;
        }
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            user.Id = global::MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            await _users.InsertOneAsync(user, cancellationToken: cancellationToken);
            _logger.LogInformation("User created: {Email}", user.Email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", user.Email);
            throw;
        }
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            await _users.ReplaceOneAsync(
                u => u.Id == user.Id,
                user,
                cancellationToken: cancellationToken);
            
            _logger.LogInformation("User updated: {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {Email}", user.Email);
            throw;
        }
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _users.DeleteOneAsync(u => u.Id == id, cancellationToken);
            _logger.LogInformation("User deleted: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {Id}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _users.CountDocumentsAsync(
                u => u.Email == email,
                cancellationToken: cancellationToken);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists: {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _users.Find(_ => true).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }
}
