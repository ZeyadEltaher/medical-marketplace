using System.Security.Claims;
using Medical.BLL.Infrastructure.RateLimit.Interfaces;
using Medical.BLL.Infrastructure.RateLimit.Models;
using Medical.BLL.Infrastructure.RateLimit.Results;
using Medical.PL.Attributes;
using Medical.PL.Options;
using Microsoft.Extensions.Options;

namespace Medical.PL.Middlewares
{
    public sealed class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRateLimiter _rateLimiter;
        private readonly RateLimitOptions _options;

        public RateLimitMiddleware(
            RequestDelegate next,
            IRateLimiter rateLimiter,
            IOptions<RateLimitOptions> options)
        {
            _next = next;
            _rateLimiter = rateLimiter;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Identify the request owner.
            var (identityType, identityValue) = GetIdentity(context);

            // 2. Apply the global policy.
            await ApplyPolicyAsync(context, "global", identityType, identityValue, _options.GlobalPolicy);

            if (context.Response.HasStarted) return;

            // 3. Check whether the endpoint has an additional policy.
            RateLimitAttribute? attribute = context.GetEndpoint()?.Metadata.GetMetadata<RateLimitAttribute>();

            if (attribute is not null)
            {
                if (!_options.Policies.TryGetValue(attribute.PolicyName, out RateLimitPolicy? policy))
                    throw new InvalidOperationException($"Rate limit policy '{attribute.PolicyName}' was not found.");

                // 4. Apply the endpoint-specific policy.
                await ApplyPolicyAsync(context, attribute.PolicyName, identityType, identityValue, policy);
            }

            // 5. Continue the HTTP pipeline.
            if (!context.Response.HasStarted)
                await _next(context);
        }


        // -- Helper Functions --

        private async Task ApplyPolicyAsync(HttpContext context, string policyName, string identityType, string identityValue, RateLimitPolicy policy)
        {
            // Validate policy configuration.
            if (policy.Limit <= 0 || policy.Window <= TimeSpan.Zero)
                throw new InvalidOperationException($"Invalid rate limit configuration for '{policyName}'.");

            // Construct a unique key for this identity and policy.
            string key = $"rate-limit:{policyName}:{identityType}:{identityValue}";

            // Ask the distributed rate limiter.
            RateLimitResult result = await _rateLimiter.TryAcquireAsync(key, policy.Limit, policy.Window);

            // Update response headers.
            context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, result.Limit - result.CurrentCount).ToString();

            // Reject the request if the limit is exceeded.
            if (!result.IsAllowed)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                long retryAfterSeconds = Math.Max(1, (long)Math.Ceiling(result.RetryAfter.TotalSeconds));
                context.Response.Headers.RetryAfter = retryAfterSeconds.ToString();

                await context.Response.WriteAsJsonAsync(new{
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Message = "Too many requests. Please try again later.",
                    RetryAfterSeconds = retryAfterSeconds
                });
            }
        }

        private static (string Type, string Value) GetIdentity(HttpContext context)
        {
            // Prefer User ID for authenticated users.
            if (context.User.Identity?.IsAuthenticated == true)
            {
                string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return ("user", userId);
                }
            }

            // Use IP address for anonymous users.
            string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return ("ip", ipAddress);
        }
    }
}