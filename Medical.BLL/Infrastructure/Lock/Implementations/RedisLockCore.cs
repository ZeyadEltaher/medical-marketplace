using StackExchange.Redis;
using Medical.BLL.Infrastructure.Lock.Constants;

namespace Medical.BLL.Infrastructure.Lock.Implementations
{
    /// <summary>
    /// Owns all direct Redis/Lua operations. It does not manage waiting, deadlines, or Pub/Sub.
    /// </summary>
    public class RedisLockCore : ILockCore
    {
        private readonly IDatabase _database;
        public RedisLockCore(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task<int> EvaluateAsync(string script, RedisKey[] keys, RedisValue[] args)
        {
            RedisResult result = await _database.ScriptEvaluateAsync(script, keys, args).ConfigureAwait(false);
            return (int)result;
        }

        public Task<int> AcquireAsync(LockRequest request) =>
            EvaluateAsync(LockScripts.Acquire, request.Keys, request.AcquireArgs);

        public Task<int> PollAsync(LockRequest request) =>
            EvaluateAsync(LockScripts.Poll, request.Keys, request.PollArgs);

        public Task<int> CancelAsync(LockRequest request) =>  
            EvaluateAsync(LockScripts.Cancel, request.Keys, request.CancelArgs);

        public Task<int> RenewAsync(LockRequest request) =>
            EvaluateAsync(LockScripts.Renew, request.Keys, request.RenewArgs);

        public Task<int> ReleaseAsync(LockRequest request) =>
            EvaluateAsync(LockScripts.Release, request.Keys, request.ReleaseArgs);
    }
}
