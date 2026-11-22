using Medical.BLL.Infrastructure.RateLimit.Results;
using Medical.BLL.Infrastructure.RateLimit.Interfaces;
using StackExchange.Redis;

namespace Medical.BLL.Infrastructure.RateLimit.Implementations
{
    public sealed class RedisRateLimiter : IRateLimiter
    {
        private readonly IDatabase _database;

        private const string Script = """
            local count = redis.call('INCR', KEYS[1])

            if count == 1 then
                redis.call('PEXPIRE', KEYS[1], ARGV[1])
            end

            local ttl = redis.call('PTTL', KEYS[1])

            return { count , ttl }
            """;

        public RedisRateLimiter(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<RateLimitResult> TryAcquireAsync(string key, int limit, TimeSpan window)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be empty.", nameof(key));

            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit));

            if (window <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(window));

            var result = await _database.ScriptEvaluateAsync(Script, [key], [(long)window.TotalMilliseconds]);

            if (result.IsNull)
                throw new InvalidOperationException("Redis returned a null result.");

            RedisResult[] values = (RedisResult[])result!;

            long currentCount = (long)values[0];
            long ttlMilliseconds = (long)values[1];

            bool isAllowed = currentCount <= limit;

            return new RateLimitResult(
                isAllowed,
                currentCount,
                limit,
                isAllowed ? TimeSpan.Zero : TimeSpan.FromMilliseconds(Math.Max(0, ttlMilliseconds))
            );
        }
    }
}

