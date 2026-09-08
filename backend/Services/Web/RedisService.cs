using StackExchange.Redis;
using System.Text.Json;


namespace backend.Services.Web
{
    public class RedisService
    {
        private readonly IDatabase _database;

        public RedisService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        /// <summary>
        /// 设置缓存对象（默认过期时间可选）
        /// </summary>
        public async Task SetCacheAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiry);
        }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        public async Task<T?> GetCacheAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        public async Task<bool> DeleteAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        /// <summary>
        /// 判断是否存在 key
        /// </summary>
        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        /// <summary>
        /// 设置过期时间
        /// </summary>
        public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
        {
            return await _database.KeyExpireAsync(key, expiry);
        }

        /// <summary>
        /// 获取剩余时间
        /// </summary>
        public async Task<TimeSpan?> GetTtlAsync(string key)
        {
            return await _database.KeyTimeToLiveAsync(key);
        }
    }

}