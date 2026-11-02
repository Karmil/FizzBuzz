using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FizzBuzz.IntegrationTests;

/// <summary>Hosts the real API in memory with test-friendly settings.</summary>
public sealed class FizzBuzzApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // TestServer has no HTTPS endpoint, and tests must not be throttled by the rate limiter.
        builder.UseSetting("Https:RedirectionEnabled", "false");
        builder.UseSetting("RateLimiting:PermitLimit", "100000");
    }
}
