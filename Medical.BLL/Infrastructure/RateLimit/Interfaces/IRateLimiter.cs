using Medical.BLL.Infrastructure.RateLimit.Results;

namespace Medical.BLL.Infrastructure.RateLimit.Interfaces
{
    public interface IRateLimiter
    {
        Task<RateLimitResult> TryAcquireAsync(
            string key,
            int limit,
            TimeSpan window
        );
    }
}

