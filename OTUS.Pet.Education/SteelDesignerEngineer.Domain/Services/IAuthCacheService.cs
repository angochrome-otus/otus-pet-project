using SteelDesignerEngineer.Domain.Entities;

namespace SteelDesignerEngineer.Domain.Services;

/// <summary>
/// Сервис для работы с кешем авторизации через Redis
/// Domain Layer - интерфейс
/// Redis используется ТОЛЬКО для кеширования, MongoDB - источник истины!
/// </summary>
public interface IAuthCacheService
{
    /// <summary>
    /// Кешировать Refresh Token в Redis для быстрой проверки
    /// Source of Truth: MongoDB (RefreshTokenRepository)
    /// TTL = срок действия токена (30 дней)
    /// </summary>
    Task CacheRefreshTokenAsync(string token, string userId, TimeSpan expiration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить UserId по Refresh Token из Redis кеша
    /// Если НЕТ в Redis ? получить из MongoDB
    /// </summary>
    Task<string?> GetCachedUserIdByRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удалить Refresh Token из Redis кеша
    /// MongoDB не трогаем! Там остается isRevoked=true
    /// </summary>
    Task RemoveCachedRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Кешировать данные пользователя в Redis для быстрого доступа
    /// Source of Truth: MongoDB (UserRepository)
    /// TTL = 15 минут
    /// </summary>
    Task CacheUserAsync(User user, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить кешированные данные пользователя из Redis
    /// Если НЕТ в Redis ? получить из MongoDB
    /// </summary>
    Task<User?> GetCachedUserAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удалить данные пользователя из Redis кеша
    /// Используется при обновлении профиля чтобы обновить кеш
    /// </summary>
    Task InvalidateUserCacheAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Добавить JWT Access Token в blacklist Redis (при logout)
    /// TTL = оставшееся время жизни токена
    /// </summary>
    Task BlacklistAccessTokenAsync(string token, TimeSpan remainingTime, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверить находится ли JWT Access Token в blacklist Redis
    /// </summary>
    Task<bool> IsAccessTokenBlacklistedAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Записать попытку входа в Redis для rate limiting
    /// TTL = 15 минут
    /// </summary>
    Task RecordLoginAttemptAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить количество неудачных попыток входа из Redis
    /// </summary>
    Task<int> GetLoginAttemptsCountAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Сбросить счетчик попыток входа в Redis (при успешном входе)
    /// </summary>
    Task ResetLoginAttemptsAsync(string email, CancellationToken cancellationToken = default);
}
