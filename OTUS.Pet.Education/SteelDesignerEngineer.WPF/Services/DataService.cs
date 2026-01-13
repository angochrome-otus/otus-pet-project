using SteelDesignerEngineer.Domain.Entities;
using SteelDesignerEngineer.Domain.Repositories;
using SteelDesignerEngineer.Domain.Services;
using Microsoft.Extensions.Logging;

namespace SteelDesignerEngineer.WPF.Services;

/// <summary>
/// Реализация сервиса для прямой работы с базами данных из WPF
/// </summary>
public class DataService : IDataService
{
    private readonly IPageContentRepository _pageContentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IMessageBus _messageBus;
    private readonly ISessionService _sessionService;
    private readonly ILogger<DataService> _logger;

    public DataService(
        IPageContentRepository pageContentRepository,
        IUserRepository userRepository,
        ICacheService cacheService,
        IMessageBus messageBus,
        ISessionService sessionService,
        ILogger<DataService> logger)
    {
        _pageContentRepository = pageContentRepository;
        _userRepository = userRepository;
        _cacheService = cacheService;
        _messageBus = messageBus;
        _sessionService = sessionService;
        _logger = logger;
    }

    #region MongoDB - PageContent

    public async Task<List<PageContent>> GetAllPagesAsync()
    {
        try
        {
            var pages = await _pageContentRepository.GetAllAsync();
            return pages.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all pages from MongoDB");
            throw;
        }
    }

    public async Task<PageContent?> GetPageByNameAsync(string pageName)
    {
        try
        {
            return await _pageContentRepository.GetByPageNameAsync(pageName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting page {PageName} from MongoDB", pageName);
            throw;
        }
    }

    public async Task<PageContent> CreatePageAsync(PageContent page)
    {
        try
        {
            page.LastModified = DateTime.UtcNow;
            
            var createdPage = await _pageContentRepository.CreateAsync(page);
            
            // Публикуем событие в RabbitMQ
            await PublishMessageAsync("page.created", new { PageName = page.PageName, Title = page.Title });
            
            _logger.LogInformation("Page created: {PageName}", page.PageName);
            return createdPage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating page {PageName}", page.PageName);
            throw;
        }
    }

    public async Task<PageContent> UpdatePageAsync(PageContent page)
    {
        try
        {
            page.LastModified = DateTime.UtcNow;
            
            var updatedPage = await _pageContentRepository.UpdateAsync(page.Id, page);
            
            // Инвалидируем кэш
            await RemoveCacheAsync($"page:{page.PageName}");
            
            // Публикуем событие в RabbitMQ
            await PublishMessageAsync("page.updated", new { PageName = page.PageName, Title = page.Title });
            
            _logger.LogInformation("Page updated: {PageName}", page.PageName);
            return updatedPage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating page {PageName}", page.PageName);
            throw;
        }
    }

    public async Task DeletePageAsync(string pageName)
    {
        try
        {
            var page = await _pageContentRepository.GetByPageNameAsync(pageName);
            if (page != null)
            {
                await _pageContentRepository.DeleteAsync(page.Id);
                
                // Инвалидируем кэш
                await RemoveCacheAsync($"page:{pageName}");
                
                // Публикуем событие в RabbitMQ
                await PublishMessageAsync("page.deleted", new { PageName = pageName });
                
                _logger.LogInformation("Page deleted: {PageName}", pageName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting page {PageName}", pageName);
            throw;
        }
    }

    #endregion

    #region MongoDB - Users

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        try
        {
            return await _userRepository.GetByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId} from MongoDB", userId);
            throw;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _userRepository.GetByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email} from MongoDB", email);
            throw;
        }
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        try
        {
            // IUserRepository не имеет GetAllAsync, поэтому нужно реализовать через другой способ
            // Для демонстрации можно оставить заглушку или добавить метод в репозиторий
            _logger.LogWarning("GetAllUsersAsync не реализован в IUserRepository");
            return new List<User>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users from MongoDB");
            throw;
        }
    }

    #endregion

    #region Redis Cache

    public async Task<T?> GetFromCacheAsync<T>(string key) where T : class
    {
        try
        {
            var json = await _cacheService.GetAsync(key);
            if (string.IsNullOrEmpty(json))
                return null;

            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache key {Key} from Redis", key);
            return null;
        }
    }

    public async Task SetCacheAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            
            if (expiration.HasValue)
            {
                await _cacheService.SetAsync(key, json, expiration.Value);
            }
            else
            {
                await _cacheService.SetAsync(key, json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key {Key} in Redis", key);
            throw;
        }
    }

    public async Task RemoveCacheAsync(string key)
    {
        try
        {
            await _cacheService.DeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key {Key} from Redis", key);
            throw;
        }
    }

    public async Task<bool> CacheExistsAsync(string key)
    {
        try
        {
            return await _cacheService.ExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache key {Key} in Redis", key);
            return false;
        }
    }

    #endregion

    #region RabbitMQ

    public async Task PublishMessageAsync(string routingKey, object message)
    {
        try
        {
            await _messageBus.PublishAsync(routingKey, message);
            _logger.LogInformation("Message published to RabbitMQ: {RoutingKey}", routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to RabbitMQ: {RoutingKey}", routingKey);
            // Не бросаем исключение, чтобы не прерывать основной процесс
        }
    }

    #endregion

    #region Session

    public async Task<string> CreateSessionAsync(string userId, TimeSpan expiration, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            // For WPF, we don't have full user info at this point
            // Using placeholder values - should be refactored to pass full user info
            return await _sessionService.CreateSessionAsync(
                userId, 
                expiration,
                "WPF User", // Placeholder
                "wpf@local",  // Placeholder
                "Unknown",    // Placeholder
                ipAddress, 
                userAgent
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ValidateSessionAsync(string sessionId)
    {
        try
        {
            var userId = await _sessionService.GetUserIdFromSessionAsync(sessionId);
            return !string.IsNullOrEmpty(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task InvalidateSessionAsync(string sessionId)
    {
        try
        {
            await _sessionService.DeleteSessionAsync(sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating session {SessionId}", sessionId);
            throw;
        }
    }

    #endregion
}
