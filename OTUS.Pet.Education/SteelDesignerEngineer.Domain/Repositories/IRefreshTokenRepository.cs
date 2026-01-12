using SteelDesignerEngineer.Domain.Entities;

namespace SteelDesignerEngineer.Domain.Repositories;

/// <summary>
/// Репозиторий для работы с Refresh Tokens в MongoDB
/// Data Layer - хранение и управление токенами
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Сохранить refresh token в MongoDB
    /// </summary>
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить refresh token из MongoDB
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить все активные токены пользователя из MongoDB
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отозвать токен в MongoDB (при выходе)
    /// </summary>
    Task RevokeAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Отозвать все токены пользователя в MongoDB
    /// </summary>
    Task RevokeAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удалить истекшие токены из MongoDB (cleanup)
    /// </summary>
    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
