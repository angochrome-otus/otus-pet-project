using MongoDB.Driver;

namespace SteelDesignerEngineer.Services.Auth.Persistence;

internal sealed class UserRepository
{
    private readonly IMongoCollection<UserDocument> _users;

    public UserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<UserDocument>("Users");

        var emailIndex = new CreateIndexModel<UserDocument>(
            Builders<UserDocument>.IndexKeys.Ascending(x => x.Email),
            new CreateIndexOptions { Unique = true });

        _users.Indexes.CreateOne(emailIndex);
    }

    public Task<UserDocument?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _users.Find(x => x.Email == email).FirstOrDefaultAsync(ct);

    public Task<UserDocument?> GetByIdAsync(string id, CancellationToken ct = default)
        => _users.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<UserDocument> CreateAsync(UserDocument user, CancellationToken ct = default)
    {
        await _users.InsertOneAsync(user, cancellationToken: ct);
        return user;
    }

    public Task UpdateAsync(UserDocument user, CancellationToken ct = default)
        => _users.ReplaceOneAsync(x => x.Id == user.Id, user, cancellationToken: ct);
}

internal sealed class UserDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "student";
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }

    public string? AuthProvider { get; set; }
    public string? OAuthProviderId { get; set; }
    public string? AvatarUrl { get; set; }
}
