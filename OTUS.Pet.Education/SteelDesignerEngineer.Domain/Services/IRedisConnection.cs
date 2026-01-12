namespace SteelDesignerEngineer.Domain.Services
{
    /// <summary>
    /// Интерфейс для Redis подключения (DIP)
    /// </summary>
    public interface IRedisConnection
    {
        Task<bool> SetAsync(string key, string value);
        Task<bool> SetAsync(string key, string value, TimeSpan expiration);
        Task<string?> GetAsync(string key);
        Task<bool> RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<bool> IsConnectedAsync();
    }
}
