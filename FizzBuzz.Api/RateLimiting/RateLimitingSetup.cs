using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FizzBuzz.Api.RateLimiting;

/// <summary>Fixed-window rate limit applied per client IP address, bound to the <c>RateLimiting</c> section.</summary>
public sealed class RateLimitingOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "RateLimiting";

    /// <summary>Requests allowed per client and per window.</summary>
    [Range(1, 100_000)]
    public int PermitLimit { get; set; } = 100;

    /// <summary>Window duration in seconds.</summary>
    [Range(1, 3_600)]
    public int WindowSeconds { get; set; } = 10;
}

internal static class RateLimitingSetup
{
    public const string FizzBuzzPolicy = "fizzbuzz";

    public static IServiceCollection AddFizzBuzzRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Behind a reverse proxy, RemoteIpAddress is the real client only when forwarded headers
            // are enabled (ASPNETCORE_FORWARDEDHEADERS_ENABLED=true, see compose.yaml).
            limiter.AddPolicy(FizzBuzzPolicy, httpContext =>
            {
                var options = httpContext.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.WindowSeconds),
                        QueueLimit = 0,
                    });
            });

            limiter.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                }

                var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too many requests.",
                        Detail = "The request rate limit has been exceeded. Please retry later.",
                    },
                });
            };
        });
    }
}
