using Medical.BLL.Infrastructure.RateLimit.Models;

namespace Medical.PL.Options
{
    public sealed class RateLimitOptions
    {
        public RateLimitPolicy GlobalPolicy { get; set; } = new();

        public Dictionary<string, RateLimitPolicy> Policies { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}