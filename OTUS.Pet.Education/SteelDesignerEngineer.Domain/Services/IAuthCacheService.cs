using SteelDesignerEngineer.Domain.Entities;

namespace SteelDesignerEngineer.Domain.Services;

/// <summary>
/// Сервис кэширования для аутентификации в Redis
/// Domain Layer - Interface
/// SESSION-BASED AUTHENTICATION ONLY - NO JWT/REFRESH TOKENS
/// </summary>
public interface IAuthCacheService
{
    /// <summary>
    /// Кэшировать пользователя в Redis (15 минут)
    /// Используется для быстрого доступа к данным пользователя без обращения к MongoDB
    /// </summary>
    Task CacheUserAsync(User user, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получить пользователя из Redis кэша
    /// Если нет в кэше - вернет null (нужно запросить из MongoDB)
    /// </summary>
    Task<User?> GetCachedUserAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Инвалидировать кэш пользователя в Redis
    /// Используется при обновлении профиля или смене пароля
    /// </summary>
    Task InvalidateUserCacheAsync(string userId, CancellationToken cancellationToken = default);
    
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
    /// Сбросить счетчик попыток входа в Redis (после успешного входа)
    /// </summary>
    Task ResetLoginAttemptsAsync(string email, CancellationToken cancellationToken = default);
}
