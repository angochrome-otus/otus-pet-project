using StackExchange.Redis;

namespace SteelDesignerEngineer.Services.Session.Persistence;

internal sealed class SessionStore
{
    private const string Prefix = "session:";
    private readonly IDatabase _db;

    public SessionStore(IConnectionMultiplexer multiplexer)
    {
        _db = multiplexer.GetDatabase();
    }

    public async Task CreateAsync(string sessionId, SessionData data, TimeSpan ttl, CancellationToken ct = default)
    {
        var key = Prefix + sessionId;
        var hashEntries = new HashEntry[]
        {
            new("userId", data.UserId),
            new("userName", data.UserName ?? string.Empty),
            new("userEmail", data.UserEmail ?? string.Empty),
            new("userRole", data.UserRole ?? string.Empty),
            new("ip", data.IpAddress ?? string.Empty),
            new("ua", data.UserAgent ?? string.Empty)
        };

        var tran = _db.CreateTransaction();
        _ = tran.HashSetAsync(key, hashEntries);
        _ = tran.KeyExpireAsync(key, ttl);
        await tran.ExecuteAsync();
    }

    public async Task<SessionData?> GetAsync(string sessionId, CancellationToken ct = default)
    {
        var key = Prefix + sessionId;
        if (!await _db.KeyExistsAsync(key))
            return null;

        var values = await _db.HashGetAllAsync(key);
        if (values.Length == 0)
            return null;

        string? Get(string name) => values.FirstOrDefault(x => x.Name == name).Value.ToString();

        return new SessionData
        {
            SessionId = sessionId,
            UserId = Get("userId") ?? string.Empty,
            UserName = Get("userName"),
            UserEmail = Get("userEmail"),
            UserRole = Get("userRole"),
            IpAddress = Get("ip"),
            UserAgent = Get("ua")
        };
    }

    public Task<bool> DeleteAsync(string sessionId)
        => _db.KeyDeleteAsync(Prefix + sessionId);

    public Task<bool> ExtendAsync(string sessionId, TimeSpan ttl)
        => _db.KeyExpireAsync(Prefix + sessionId, ttl);
}

internal sealed class SessionData
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
