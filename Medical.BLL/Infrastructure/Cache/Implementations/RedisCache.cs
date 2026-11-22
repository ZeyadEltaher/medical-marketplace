using System.Text.Json;
using StackExchange.Redis;
using Medical.BLL.Infrastructure.Cache.Interfaces;

namespace Medical.BLL.Infrastructure.Cache.Implementations
{
    public class RedisCache : IDistributedCache
    {
        private readonly IDatabase _database;
        public RedisCache(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<bool> StringSetAsync<T>(string key, T value, TimeSpan expiry)
        {
            string json = JsonSerializer.Serialize(value);

            RedisKey redisKey = key;

            RedisValue redisValue = json;

            return await _database.StringSetAsync(redisKey, redisValue, expiry);
        }

        public async Task<T?> StringGetAsync<T>(string key)
        {
            RedisKey redisKey = key;

            RedisValue redisValue = await _database.StringGetAsync(redisKey);

            if (redisValue.IsNull)
            {
                return default;
            }

            string value = redisValue!; 

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task<bool> StringSetBytesAsync(string key, byte[] bytes, TimeSpan expiry)
        {
            RedisKey redisKey = key;

            RedisValue redisValue = bytes;

            return await _database.StringSetAsync(redisKey, redisValue, expiry);
        }

        public async Task<bool> KeyExpireAsync(string key, TimeSpan expiry)
        {
            RedisKey redisKey = key;
            return await _database.KeyExpireAsync(redisKey, expiry);
        }


        public async Task<byte[]?> StringGetBytesAsync(string key)
        {
            RedisKey redisKey = key;

            RedisValue redisValue = await _database.StringGetAsync(redisKey);

            if (redisValue.IsNull)
            {
                return default;
            }

            byte[] value = redisValue!;

            return value;
        }

        public async Task<bool> DeleteAsync(string key)
        {
            RedisKey redisKey = key;

            return await _database.KeyDeleteAsync(redisKey);
        }

    }
}