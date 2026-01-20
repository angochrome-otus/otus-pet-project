using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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
            new CreateIndexOptions { Unique = true, Name = "ux_users_email" });

        try
        {
            _users.Indexes.CreateOne(emailIndex);
        }
        catch (MongoCommandException ex) when (
            ex.Message.Contains("Index already exists", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            // ignore: another initializer already created a unique index for Email
        }
    }

    public Task<UserDocument?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _users.Find(x => x.Email == email).FirstOrDefaultAsync(ct);

    public Task<UserDocument?> GetByIdAsync(string id, CancellationToken ct = default)
        => _users.Find(x => x.MongoId == id).FirstOrDefaultAsync(ct);

    public async Task<UserDocument> CreateAsync(UserDocument user, CancellationToken ct = default)
    {
        await _users.InsertOneAsync(user, cancellationToken: ct);
        return user;
    }

    public Task UpdateAsync(UserDocument user, CancellationToken ct = default)
        => _users.ReplaceOneAsync(x => x.MongoId == user.MongoId, user, cancellationToken: ct);
}

internal sealed class UserDocument
{
    // Primary MongoDB document id (_id)
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string MongoId { get; set; } = ObjectId.GenerateNewId().ToString();

    // Legacy field used previously in this project. Keep it to allow reading old documents.
    [BsonElement("Id")]
    [BsonIgnoreIfNull]
    public string? LegacyId { get; set; }

    [BsonIgnore]
    public string Id
    {
        get => MongoId;
        set => MongoId = value;
    }

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
