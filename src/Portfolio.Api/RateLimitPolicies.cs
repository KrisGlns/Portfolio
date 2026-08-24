using System.Threading.RateLimiting;

namespace Portfolio.Api;

public static class RateLimitPolicies
{
    public const string Contact = "contact";

    /// <summary>Five submissions per IP per fifteen minutes — generous for a human, useless for a spammer.</summary>
    public static void AddPortfolioRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(Contact, http => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0
                }));
        });
}
