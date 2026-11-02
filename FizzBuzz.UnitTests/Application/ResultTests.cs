using FizzBuzz.Application.Common;

namespace FizzBuzz.UnitTests.Application;

public sealed class ResultTests
{
    [Fact]
    public void Success_ExposesValueAndNoErrors()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Invalid_ExposesErrorsAndRefusesValueAccess()
    {
        var errors = new Dictionary<string, string[]> { ["Limit"] = ["Must be between 1 and 10000."] };

        var result = Result.Invalid<int>(errors);

        Assert.False(result.IsSuccess);
        Assert.Same(errors, result.Errors);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Invalid_WithoutErrors_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result.Invalid<int>(new Dictionary<string, string[]>()));
    }
}
