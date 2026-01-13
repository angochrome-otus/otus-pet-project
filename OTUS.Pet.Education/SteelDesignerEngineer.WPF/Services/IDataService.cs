using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Интерфейс для прямой работы с базами данных из WPF
/// </summary>
public interface IDataService
{
    // MongoDB - PageContent
    Task<List<PageContent>> GetAllPagesAsync();
    Task<PageContent?> GetPageByNameAsync(string pageName);
    Task<PageContent> CreatePageAsync(PageContent page);
    Task<PageContent> UpdatePageAsync(PageContent page);
    Task DeletePageAsync(string pageName);
    
    // MongoDB - Users
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<List<User>> GetAllUsersAsync();
    
    // Redis Cache
    Task<T?> GetFromCacheAsync<T>(string key) where T : class;
    Task SetCacheAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveCacheAsync(string key);
    Task<bool> CacheExistsAsync(string key);
    
    // RabbitMQ
    Task PublishMessageAsync(string routingKey, object message);
    
    // Session
    Task<string> CreateSessionAsync(string userId, TimeSpan expiration, string? ipAddress = null, string? userAgent = null);
    Task<bool> ValidateSessionAsync(string sessionId);
    Task InvalidateSessionAsync(string sessionId);
}
