using System.Net.Mime;
using System.Text.Json;
using FizzBuzz.Application.Features.GenerateFizzBuzz;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FizzBuzz.Api.HealthChecks;

/// <summary>
/// Liveness (<c>/health/live</c>): the process answers HTTP requests.
/// Readiness (<c>/health/ready</c>): the FizzBuzz feature is configured and produces the expected sequence.
/// </summary>
internal static class HealthChecksSetup
{
    private const string LiveTag = "live";
    private const string ReadyTag = "ready";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IServiceCollection AddFizzBuzzHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: [LiveTag])
            .AddCheck<FizzBuzzReadinessCheck>("fizzbuzz", tags: [ReadyTag]);

        return services;
    }

    public static IEndpointRouteBuilder MapFizzBuzzHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(LiveTag),
            ResponseWriter = WriteJsonResponse,
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(ReadyTag),
            ResponseWriter = WriteJsonResponse,
        });

        return endpoints;
    }

    private static Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
            }),
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}

/// <summary>Runs the use case on a known input and compares the result with the expected output.</summary>
internal sealed class FizzBuzzReadinessCheck(IGenerateFizzBuzzUseCase generateFizzBuzz) : IHealthCheck
{
    private static readonly GenerateFizzBuzzQuery Probe = new(3, 5, 15, "Fizz", "Buzz");

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var result = generateFizzBuzz.Execute(Probe);

        var healthy = result.IsSuccess && result.Value is [.., "14", "FizzBuzz"];
        return Task.FromResult(healthy
            ? HealthCheckResult.Healthy("FizzBuzz generation is operational.")
            : HealthCheckResult.Unhealthy("FizzBuzz generation returned an unexpected result."));
    }
}
