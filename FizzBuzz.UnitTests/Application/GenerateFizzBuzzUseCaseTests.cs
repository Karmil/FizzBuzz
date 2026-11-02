using FizzBuzz.Application;
using FizzBuzz.Application.Features.GenerateFizzBuzz;
using FizzBuzz.Domain;
using Microsoft.Extensions.Options;

namespace FizzBuzz.UnitTests.Application;

public sealed class GenerateFizzBuzzUseCaseTests
{
    private static GenerateFizzBuzzUseCase CreateUseCase(int maxLimit = FizzBuzzGenerator.MaxLimit) =>
        new(new GenerateFizzBuzzQueryValidator(Options.Create(new FizzBuzzOptions { MaxLimit = maxLimit })));

    private static GenerateFizzBuzzQuery ValidQuery() => new(3, 5, 15, "Fizz", "Buzz");

    [Fact]
    public void Execute_ValidQuery_ReturnsSequence()
    {
        // Arrange
        var useCase = CreateUseCase();

        // Act
        var result = useCase.Execute(ValidQuery());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(
            ["1", "2", "Fizz", "4", "Buzz", "Fizz", "7", "8", "Fizz", "Buzz", "11", "Fizz", "13", "14", "FizzBuzz"],
            result.Value);
    }

    [Fact]
    public void Execute_IdenticalDivisors_IsAllowed()
    {
        var result = CreateUseCase().Execute(ValidQuery() with { Int2 = 3 });

        Assert.True(result.IsSuccess);
        Assert.Equal("FizzBuzz", result.Value[2]);
    }

    [Fact]
    public void Execute_UnicodeReplacements_ArePreserved()
    {
        var result = CreateUseCase().Execute(ValidQuery() with { Str1 = "Fïzz", Str2 = "🐝" });

        Assert.True(result.IsSuccess);
        Assert.Equal("Fïzz🐝", result.Value[14]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Execute_InvalidFirstDivisor_ReturnsError(int divisor)
    {
        var result = CreateUseCase().Execute(ValidQuery() with { Int1 = divisor });

        AssertSingleError(result.Errors, nameof(GenerateFizzBuzzQuery.Int1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Execute_InvalidSecondDivisor_ReturnsError(int divisor)
    {
        var result = CreateUseCase().Execute(ValidQuery() with { Int2 = divisor });

        AssertSingleError(result.Errors, nameof(GenerateFizzBuzzQuery.Int2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(FizzBuzzGenerator.MaxLimit + 1)]
    public void Execute_LimitOutOfRange_ReturnsError(int limit)
    {
        var result = CreateUseCase().Execute(ValidQuery() with { Limit = limit });

        AssertSingleError(result.Errors, nameof(GenerateFizzBuzzQuery.Limit));
    }

    [Fact]
    public void Execute_LimitAboveConfiguredMaximum_ReturnsError()
    {
        var useCase = CreateUseCase(maxLimit: 100);

        var result = useCase.Execute(ValidQuery() with { Limit = 101 });

        AssertSingleError(result.Errors, nameof(GenerateFizzBuzzQuery.Limit));
        Assert.Contains("100", result.Errors[nameof(GenerateFizzBuzzQuery.Limit)][0], StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_LimitEqualToConfiguredMaximum_Succeeds()
    {
        var result = CreateUseCase(maxLimit: 100).Execute(ValidQuery() with { Limit = 100 });

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Execute_EmptyReplacement_ReturnsError(string? replacement)
    {
        var result = CreateUseCase().Execute(ValidQuery() with { Str1 = replacement! });

        AssertSingleError(result.Errors, nameof(GenerateFizzBuzzQuery.Str1));
    }

    [Fact]
    public void Execute_ReplacementTooLong_ReturnsError()
    {
        var tooLong = new string('x', FizzBuzzRule.MaxReplacementLength + 1);

        var result = CreateUseCase().Execute(ValidQuery() with { Str2 = tooLong });

        AssertSingleError(result.Errors, nameof(GenerateFizzBuzzQuery.Str2));
    }

    [Fact]
    public void Execute_SeveralInvalidValues_ReportsAllErrors()
    {
        var query = new GenerateFizzBuzzQuery(0, -1, 0, "", new string('x', 51));

        var result = CreateUseCase().Execute(query);

        Assert.False(result.IsSuccess);
        Assert.Equal(["Int1", "Int2", "Limit", "Str1", "Str2"], result.Errors.Keys.Order(StringComparer.Ordinal));
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Execute_NullQuery_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CreateUseCase().Execute(null!));
    }

    private static void AssertSingleError(IReadOnlyDictionary<string, string[]> errors, string expectedKey)
    {
        var error = Assert.Single(errors);
        Assert.Equal(expectedKey, error.Key);
        Assert.NotEmpty(error.Value);
    }
}
