using SteelDesignerEngineer.Domain.Services;
using System.Threading;

namespace SteelDesignerEngineer.Infrastructure.Caching
{
    /// <summary>
    /// Реализация кеш сервиса через Redis с DIP
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IRedisConnection _redisConnection;

        public RedisCacheService(IRedisConnection redisConnection)
        {
            _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        }

        /// <summary>
        /// Получить значение из Redis
        /// </summary>
        public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            return await _redisConnection.GetAsync(key);
        }

        /// <summary>
        /// Сохранить значение в Redis без TTL
        /// </summary>
        public async Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            await _redisConnection.SetAsync(key, value);
        }

        /// <summary>
        /// Сохранить значение в Redis с TTL
        /// </summary>
        public async Task SetAsync(string key, string value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            await _redisConnection.SetAsync(key, value, expiration);
        }

        /// <summary>
        /// Удалить значение из Redis
        /// </summary>
        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            await _redisConnection.RemoveAsync(key);
        }

        /// <summary>
        /// Удалить значение из Redis (для обратной совместимости)
        /// </summary>
        public async Task RemoveDataAsync(string key)
        {
            await _redisConnection.RemoveAsync(key);
        }

        /// <summary>
        /// Проверить существует ли ключ в Redis
        /// </summary>
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return await _redisConnection.ExistsAsync(key);
        }
    }
}