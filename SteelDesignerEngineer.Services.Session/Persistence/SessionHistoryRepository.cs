using MongoDB.Driver;

namespace SteelDesignerEngineer.Services.Session.Persistence;

internal sealed class SessionHistoryRepository
{
    private readonly IMongoCollection<SessionHistoryDocument> _collection;

    public SessionHistoryRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<SessionHistoryDocument>("sessionHistory");
    }

    public Task InsertAsync(SessionHistoryDocument doc, CancellationToken ct = default)
        => _collection.InsertOneAsync(doc, cancellationToken: ct);

    public async Task<List<SessionHistoryDocument>> GetByUserIdAsync(string userId, int limit = 50, CancellationToken ct = default)
    {
        return await _collection.Find(x => x.UserId == userId)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync(ct);
    }
}

internal sealed class SessionHistoryDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
