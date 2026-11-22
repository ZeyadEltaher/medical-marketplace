namespace Medical.BLL.Infrastructure.RateLimit.Results
{
    public sealed record RateLimitResult
    (
        bool IsAllowed,
        long CurrentCount,
        int Limit,
        TimeSpan RetryAfter
    );
}