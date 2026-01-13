using MongoDB.Driver;
using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;

namespace SteelDesignerEngineer.Data.MongoDB;

/// <summary>
/// MongoDB реализация репозитория истории сессий
/// Clean Architecture: Data Layer
/// </summary>
public class SessionHistoryRepository : ISessionHistoryRepository
{
    private readonly IMongoCollection<SessionHistory> _sessionHistory;

    public SessionHistoryRepository(IMongoDatabase database)
    {
        _sessionHistory = database.GetCollection<SessionHistory>("SessionHistory");

        // Создаем индексы для быстрого поиска
        CreateIndexesAsync().GetAwaiter().GetResult();
    }

    private async Task CreateIndexesAsync()
    {
        // Индекс по UserId для быстрого поиска сессий пользователя
        var userIdIndex = Builders<SessionHistory>.IndexKeys.Ascending(s => s.UserId);
        await _sessionHistory.Indexes.CreateOneAsync(new CreateIndexModel<SessionHistory>(userIdIndex));

        // Индекс по SessionId для быстрого поиска по ID сессии
        var sessionIdIndex = Builders<SessionHistory>.IndexKeys.Ascending(s => s.SessionId);
        await _sessionHistory.Indexes.CreateOneAsync(new CreateIndexModel<SessionHistory>(sessionIdIndex));

        // Индекс по IsActive для поиска активных сессий
        var activeIndex = Builders<SessionHistory>.IndexKeys.Ascending(s => s.IsActive);
        await _sessionHistory.Indexes.CreateOneAsync(new CreateIndexModel<SessionHistory>(activeIndex));

        // Составной индекс для поиска активных сессий пользователя
        var userActiveIndex = Builders<SessionHistory>.IndexKeys
            .Ascending(s => s.UserId)
            .Ascending(s => s.IsActive);
        await _sessionHistory.Indexes.CreateOneAsync(new CreateIndexModel<SessionHistory>(userActiveIndex));
    }

    public async Task<SessionHistory> CreateAsync(SessionHistory session, CancellationToken cancellationToken = default)
    {
        await _sessionHistory.InsertOneAsync(session, cancellationToken: cancellationToken);
        return session;
    }

    public async Task UpdateLogoutAsync(string sessionId, DateTime logoutAt, string logoutType, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SessionHistory>.Filter.Eq(s => s.SessionId, sessionId);
        
        var session = await _sessionHistory.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (session == null) return;

        var durationSeconds = (long)(logoutAt - session.LoginAt).TotalSeconds;

        var update = Builders<SessionHistory>.Update
            .Set(s => s.LogoutAt, logoutAt)
            .Set(s => s.LogoutType, logoutType)
            .Set(s => s.DurationSeconds, durationSeconds)
            .Set(s => s.IsActive, false);

        await _sessionHistory.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task<List<SessionHistory>> GetActiveSessionsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SessionHistory>.Filter.And(
            Builders<SessionHistory>.Filter.Eq(s => s.UserId, userId),
            Builders<SessionHistory>.Filter.Eq(s => s.IsActive, true)
        );

        return await _sessionHistory
            .Find(filter)
            .SortByDescending(s => s.LoginAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SessionHistory>> GetSessionHistoryByUserIdAsync(string userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SessionHistory>.Filter.Eq(s => s.UserId, userId);

        return await _sessionHistory
            .Find(filter)
            .SortByDescending(s => s.LoginAt)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<SessionHistory?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SessionHistory>.Filter.Eq(s => s.SessionId, sessionId);
        return await _sessionHistory.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task LogoutAllSessionsAsync(string userId, DateTime logoutAt, CancellationToken cancellationToken = default)
    {
        var filter = Builders<SessionHistory>.Filter.And(
            Builders<SessionHistory>.Filter.Eq(s => s.UserId, userId),
            Builders<SessionHistory>.Filter.Eq(s => s.IsActive, true)
        );

        var sessions = await _sessionHistory.Find(filter).ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            var durationSeconds = (long)(logoutAt - session.LoginAt).TotalSeconds;

            var update = Builders<SessionHistory>.Update
                .Set(s => s.LogoutAt, logoutAt)
                .Set(s => s.LogoutType, "revoked")
                .Set(s => s.DurationSeconds, durationSeconds)
                .Set(s => s.IsActive, false);

            await _sessionHistory.UpdateOneAsync(
                Builders<SessionHistory>.Filter.Eq(s => s.Id, session.Id),
                update,
                cancellationToken: cancellationToken
            );
        }
    }
}
