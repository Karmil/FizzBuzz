using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace FizzBuzz.IntegrationTests;

public sealed class FizzBuzzEndpointTests(FizzBuzzApiFactory factory) : IClassFixture<FizzBuzzApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string BuildUrl(string int1 = "3", string int2 = "5", string limit = "15", string str1 = "Fizz", string str2 = "Buzz") =>
        $"/api/v1/fizzbuzz?int1={int1}&int2={int2}&limit={limit}"
        + $"&str1={Uri.EscapeDataString(str1)}&str2={Uri.EscapeDataString(str2)}";

    [Fact]
    public async Task Get_ValidRequest_Returns200WithExactJsonArray()
    {
        // Act
        using var response = await _client.GetAsync(BuildUrl());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(
            """["1","2","Fizz","4","Buzz","Fizz","7","8","Fizz","Buzz","11","Fizz","13","14","FizzBuzz"]""",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Get_ReservedAndUnicodeCharacters_AreDecodedCorrectly()
    {
        using var response = await _client.GetAsync(BuildUrl(int1: "2", int2: "3", limit: "6", str1: "C++ & co", str2: "日本😀"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var values = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.Equal(["1", "C++ & co", "日本😀", "C++ & co", "5", "C++ & co日本😀"], values!);
    }

    [Fact]
    public async Task Get_MaximumLimit_ReturnsAllValues()
    {
        using var response = await _client.GetAsync(BuildUrl(limit: "10000"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var values = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.Equal(10_000, values!.Length);
        Assert.Equal("Buzz", values[^1]);
    }

    [Theory]
    [InlineData("int1", "0")]
    [InlineData("int1", "-3")]
    [InlineData("int2", "0")]
    [InlineData("limit", "0")]
    [InlineData("limit", "10001")]
    [InlineData("int1", "abc")]
    [InlineData("limit", "1.5")]
    [InlineData("limit", "99999999999")]
    [InlineData("str1", "")]
    [InlineData("str2", "   ")]
    [InlineData("str1", "123456789012345678901234567890123456789012345678901")]
    public async Task Get_InvalidParameter_Returns400WithErrorForThatParameter(string parameter, string value)
    {
        // Arrange
        var url = parameter switch
        {
            "int1" => BuildUrl(int1: value),
            "int2" => BuildUrl(int2: value),
            "limit" => BuildUrl(limit: value),
            "str1" => BuildUrl(str1: value),
            _ => BuildUrl(str2: value),
        };

        // Act
        using var response = await _client.GetAsync(url);

        // Assert
        var problem = await AssertValidationProblemAsync(response);
        Assert.Equal([parameter], problem.Errors.Keys);
    }

    [Fact]
    public async Task Get_MissingParameters_Returns400ListingEachOne()
    {
        using var response = await _client.GetAsync("/api/v1/fizzbuzz");

        var problem = await AssertValidationProblemAsync(response);
        Assert.Equal(["int1", "int2", "limit", "str1", "str2"], problem.Errors.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Get_SameDivisors_IsAccepted()
    {
        using var response = await _client.GetAsync(BuildUrl(int1: "4", int2: "4", limit: "4"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["1", "2", "3", "FizzBuzz"], await response.Content.ReadFromJsonAsync<string[]>() ?? []);
    }

    [Fact]
    public async Task Get_AnyRequest_ReturnsTraceIdHeader()
    {
        using var response = await _client.GetAsync(BuildUrl());

        Assert.True(response.Headers.TryGetValues("X-Trace-Id", out var values));
        Assert.False(string.IsNullOrWhiteSpace(Assert.Single(values)));
    }

    [Fact]
    public async Task Get_UnknownRoute_Returns404ProblemDetails()
    {
        using var response = await _client.GetAsync("/api/v1/unknown");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private static async Task<ValidationProblemDetails> AssertValidationProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("One or more validation errors occurred.", problem.Title);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
        Assert.All(problem.Errors.Values, messages => Assert.NotEmpty(messages));
        return problem;
    }
}
