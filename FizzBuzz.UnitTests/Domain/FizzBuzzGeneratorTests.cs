using FizzBuzz.Domain;

namespace FizzBuzz.UnitTests.Domain;

public sealed class FizzBuzzGeneratorTests
{
    private static readonly FizzBuzzRule Fizz = new(3, "Fizz");
    private static readonly FizzBuzzRule Buzz = new(5, "Buzz");

    [Fact]
    public void Generate_ClassicParameters_ReturnsExpectedSequenceInOrder()
    {
        // Act
        var values = FizzBuzzGenerator.Generate(Fizz, Buzz, 15);

        // Assert
        string[] expected =
        [
            "1", "2", "Fizz", "4", "Buzz",
            "Fizz", "7", "8", "Fizz", "Buzz",
            "11", "Fizz", "13", "14", "FizzBuzz",
        ];
        Assert.Equal(expected, values);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(9)]
    [InlineData(99)]
    public void Generate_MultipleOfFirstDivisorOnly_ReturnsFirstReplacement(int number)
    {
        var values = FizzBuzzGenerator.Generate(Fizz, Buzz, 100);

        Assert.Equal("Fizz", values[number - 1]);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(100)]
    public void Generate_MultipleOfSecondDivisorOnly_ReturnsSecondReplacement(int number)
    {
        var values = FizzBuzzGenerator.Generate(Fizz, Buzz, 100);

        Assert.Equal("Buzz", values[number - 1]);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(90)]
    public void Generate_MultipleOfBothDivisors_ReturnsFirstThenSecondReplacement(int number)
    {
        var values = FizzBuzzGenerator.Generate(Fizz, Buzz, 100);

        Assert.Equal("FizzBuzz", values[number - 1]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(98)]
    public void Generate_NumberMatchingNoRule_ReturnsNumberAsString(int number)
    {
        var values = FizzBuzzGenerator.Generate(Fizz, Buzz, 100);

        Assert.Equal(number.ToString(System.Globalization.CultureInfo.InvariantCulture), values[number - 1]);
    }

    [Fact]
    public void Generate_ReplacementsAreConcatenatedInRuleOrder()
    {
        // Arrange: the second rule's divisor is smaller, the order must still follow the parameters.
        var first = new FizzBuzzRule(5, "Buzz");
        var second = new FizzBuzzRule(3, "Fizz");

        // Act
        var values = FizzBuzzGenerator.Generate(first, second, 15);

        // Assert
        Assert.Equal("BuzzFizz", values[14]);
    }

    [Fact]
    public void Generate_IdenticalDivisors_ReturnsConcatenationForEveryMultiple()
    {
        var values = FizzBuzzGenerator.Generate(new FizzBuzzRule(2, "a"), new FizzBuzzRule(2, "b"), 6);

        Assert.Equal(["1", "ab", "3", "ab", "5", "ab"], values);
    }

    [Fact]
    public void Generate_FirstDivisorIsOne_EveryNumberIsReplaced()
    {
        var values = FizzBuzzGenerator.Generate(new FizzBuzzRule(1, "Fizz"), Buzz, 5);

        Assert.Equal(["Fizz", "Fizz", "Fizz", "Fizz", "FizzBuzz"], values);
    }

    [Fact]
    public void Generate_BothDivisorsAreOne_EveryNumberIsTheConcatenation()
    {
        var values = FizzBuzzGenerator.Generate(new FizzBuzzRule(1, "Fizz"), new FizzBuzzRule(1, "Buzz"), 3);

        Assert.All(values, value => Assert.Equal("FizzBuzz", value));
    }

    [Fact]
    public void Generate_DivisorsGreaterThanLimit_ReturnsOnlyNumbers()
    {
        var values = FizzBuzzGenerator.Generate(new FizzBuzzRule(10, "Fizz"), new FizzBuzzRule(20, "Buzz"), 5);

        Assert.Equal(["1", "2", "3", "4", "5"], values);
    }

    [Fact]
    public void Generate_UnicodeReplacements_ArePreserved()
    {
        var values = FizzBuzzGenerator.Generate(new FizzBuzzRule(2, "é"), new FizzBuzzRule(3, "😀"), 6);

        Assert.Equal(["1", "é", "😀", "é", "5", "é😀"], values);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(FizzBuzzGenerator.MaxLimit)]
    public void Generate_LimitAtBounds_ReturnsOneValuePerNumber(int limit)
    {
        var values = FizzBuzzGenerator.Generate(Fizz, Buzz, limit);

        Assert.Equal(limit, values.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(FizzBuzzGenerator.MaxLimit + 1)]
    [InlineData(int.MaxValue)]
    public void Generate_LimitOutOfRange_Throws(int limit)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FizzBuzzGenerator.Generate(Fizz, Buzz, limit));
    }

    [Fact]
    public void Generate_NullRule_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => FizzBuzzGenerator.Generate(null!, Buzz, 15));
        Assert.Throws<ArgumentNullException>(() => FizzBuzzGenerator.Generate(Fizz, null!, 15));
    }
}
