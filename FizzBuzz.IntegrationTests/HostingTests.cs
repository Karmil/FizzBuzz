using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FizzBuzz.Application.Common;
using FizzBuzz.Application.Features.GenerateFizzBuzz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FizzBuzz.IntegrationTests;

/// <summary>Startup, health checks, configuration and cross-cutting concerns.</summary>
public sealed class HostingTests(FizzBuzzApiFactory factory) : IClassFixture<FizzBuzzApiFactory>
{
    private const string ValidUrl = "/api/v1/fizzbuzz?int1=3&int2=5&limit=15&str1=Fizz&str2=Buzz";

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoint_ApiStarted_ReturnsHealthy(string path)
    {
        // Arrange
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(path);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", body.RootElement.GetProperty("status").GetString());
        Assert.NotEqual(0, body.RootElement.GetProperty("checks").GetArrayLength());
    }

    [Fact]
    public async Task Get_LimitAboveConfiguredMaximum_Returns400()
    {
        // Arrange
        using var configured = factory.WithWebHostBuilder(builder => builder.UseSetting("FizzBuzz:MaxLimit", "100"));
        using var client = configured.CreateClient();

        // Act
        using var accepted = await client.GetAsync("/api/v1/fizzbuzz?int1=3&int2=5&limit=100&str1=a&str2=b");
        using var rejected = await client.GetAsync("/api/v1/fizzbuzz?int1=3&int2=5&limit=101&str1=a&str2=b");

        // Assert
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        var problem = await rejected.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("limit", problem!.Errors.Keys);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("10001")]
    public void Startup_MaxLimitOutsideBusinessBounds_FailsFast(string maxLimit)
    {
        using var misconfigured = factory.WithWebHostBuilder(builder => builder.UseSetting("FizzBuzz:MaxLimit", maxLimit));

        var exception = Assert.Throws<OptionsValidationException>(() => misconfigured.CreateClient());
        Assert.Contains("MaxLimit", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Get_UnexpectedException_Returns500WithoutInternalDetails()
    {
        // Arrange
        using var failing = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddSingleton<IGenerateFizzBuzzUseCase, ThrowingUseCase>()));
        using var client = failing.CreateClient();

        // Act
        using var response = await client.GetAsync(ValidUrl);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(ThrowingUseCase.SecretMessage, body, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(ThrowingUseCase), body, StringComparison.Ordinal);

        var problem = JsonSerializer.Deserialize<ProblemDetails>(body, JsonSerializerOptions.Web);
        Assert.Equal(500, problem!.Status);
        Assert.Equal("An unexpected error occurred.", problem.Title);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task Get_RateLimitExceeded_Returns429ProblemDetails()
    {
        // Arrange
        using var limited = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("RateLimiting:PermitLimit", "2");
            builder.UseSetting("RateLimiting:WindowSeconds", "60");
        });
        using var client = limited.CreateClient();

        // Act
        using var first = await client.GetAsync(ValidUrl);
        using var second = await client.GetAsync(ValidUrl);
        using var third = await client.GetAsync(ValidUrl);

        // Assert
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
        Assert.Equal("application/problem+json", third.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(third.Headers.RetryAfter);
    }

    [Fact]
    public async Task HealthEndpoint_IsNotRateLimited()
    {
        using var limited = factory.WithWebHostBuilder(builder => builder.UseSetting("RateLimiting:PermitLimit", "1"));
        using var client = limited.CreateClient();

        for (var attempt = 0; attempt < 3; attempt++)
        {
            using var response = await client.GetAsync("/health/live");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    private sealed class ThrowingUseCase : IGenerateFizzBuzzUseCase
    {
        public const string SecretMessage = "connection string=Server=secret";

        public Result<IReadOnlyList<string>> Execute(GenerateFizzBuzzQuery query) =>
            throw new InvalidOperationException(SecretMessage);
    }
}
