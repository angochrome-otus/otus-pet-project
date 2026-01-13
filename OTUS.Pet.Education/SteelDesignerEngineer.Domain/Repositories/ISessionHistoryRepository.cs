using SteelDesignerEngineer.Domain.Entities;

namespace SteelDesignerEngineer.Domain.Repositories;

/// <summary>
/// Repository для истории сессий
/// Clean Architecture: Domain Layer
/// </summary>
public interface ISessionHistoryRepository
{
    /// <summary>
    /// Создать запись о начале сессии
    /// </summary>
    Task<SessionHistory> CreateAsync(SessionHistory session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Завершить сессию
    /// </summary>
    Task UpdateLogoutAsync(string sessionId, DateTime logoutAt, string logoutType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить активные сессии пользователя
    /// </summary>
    Task<List<SessionHistory>> GetActiveSessionsByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить историю сессий пользователя
    /// </summary>
    Task<List<SessionHistory>> GetSessionHistoryByUserIdAsync(string userId, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить сессию по SessionId
    /// </summary>
    Task<SessionHistory?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Завершить все активные сессии пользователя (logout from all devices)
    /// </summary>
    Task LogoutAllSessionsAsync(string userId, DateTime logoutAt, CancellationToken cancellationToken = default);
}
