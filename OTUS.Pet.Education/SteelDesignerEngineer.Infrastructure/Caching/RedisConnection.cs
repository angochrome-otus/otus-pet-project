using SteelDesignerEngineer.Domain.Services;
using StackExchange.Redis;

namespace SteelDesignerEngineer.Infrastructure.Caching
{
    /// <summary>
    /// Реализация подключения к Redis с DIP
    /// </summary>
    public class RedisConnection : IRedisConnection
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisConnection(IConnectionMultiplexer redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _database = _redis.GetDatabase();
        }

        public async Task<bool> SetAsync(string key, string value)
        {
            return await _database.StringSetAsync(key, value);
        }

        public async Task<bool> SetAsync(string key, string value, TimeSpan expiration)
        {
            return await _database.StringSetAsync(key, value, expiration);
        }

        public async Task<string?> GetAsync(string key)
        {
            var value = await _database.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                await _database.PingAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
