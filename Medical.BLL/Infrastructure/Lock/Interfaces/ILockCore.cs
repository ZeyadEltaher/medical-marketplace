using StackExchange.Redis;
using Medical.BLL.Infrastructure.Lock.Constants;

namespace Medical.BLL.Infrastructure.Lock.Implementations
{
    public interface ILockCore
    {
        Task<int> EvaluateAsync(string script, RedisKey[] keys, RedisValue[] args);
        Task<int> AcquireAsync(LockRequest request);
        Task<int> PollAsync(LockRequest request);
        Task<int> CancelAsync(LockRequest request);
        Task<int> RenewAsync(LockRequest request);
        Task<int> ReleaseAsync(LockRequest request);
    }
}
