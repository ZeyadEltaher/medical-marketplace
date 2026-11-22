namespace Medical.BLL.Infrastructure.RateLimit.Models
{
    public sealed class RateLimitPolicy
    {
        public int Limit { get; init; }

        public TimeSpan Window { get; init; }
    }
}