using FizzBuzz.Domain;

namespace FizzBuzz.UnitTests.Domain;

public sealed class FizzBuzzRuleTests
{
    [Fact]
    public void Constructor_ValidArguments_ExposesThem()
    {
        var rule = new FizzBuzzRule(3, "Fizz");

        Assert.Equal(3, rule.Divisor);
        Assert.Equal("Fizz", rule.Replacement);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(int.MinValue)]
    public void Constructor_DivisorNotStrictlyPositive_Throws(int divisor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FizzBuzzRule(divisor, "Fizz"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\n")]
    public void Constructor_BlankReplacement_Throws(string replacement)
    {
        Assert.Throws<ArgumentException>(() => new FizzBuzzRule(3, replacement));
    }

    [Fact]
    public void Constructor_NullReplacement_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new FizzBuzzRule(3, null!));
    }

    [Fact]
    public void Constructor_ReplacementAtMaxLength_IsAccepted()
    {
        var replacement = new string('a', FizzBuzzRule.MaxReplacementLength);

        var rule = new FizzBuzzRule(3, replacement);

        Assert.Equal(replacement, rule.Replacement);
    }

    [Fact]
    public void Constructor_ReplacementTooLong_Throws()
    {
        var replacement = new string('a', FizzBuzzRule.MaxReplacementLength + 1);

        var exception = Assert.Throws<ArgumentException>(() => new FizzBuzzRule(3, replacement));
        Assert.Equal("replacement", exception.ParamName);
    }

    [Fact]
    public void Constructor_MaxLengthOfEmojis_IsAcceptedAlthoughUtf16LengthIsDouble()
    {
        // Arrange: 50 emojis = 50 characters but 100 UTF-16 code units.
        var replacement = string.Concat(Enumerable.Repeat("😀", FizzBuzzRule.MaxReplacementLength));

        // Act
        var rule = new FizzBuzzRule(3, replacement);

        // Assert
        Assert.Equal(100, rule.Replacement.Length);
    }

    [Theory]
    [InlineData("Fizz", 4)]
    [InlineData("été", 3)]
    [InlineData("😀", 1)]
    [InlineData("日本語", 3)]
    [InlineData("", 0)]
    public void CountCharacters_CountsUnicodeCodePoints(string text, int expected)
    {
        Assert.Equal(expected, FizzBuzzRule.CountCharacters(text));
    }

    [Theory]
    [InlineData(3, 9, true)]
    [InlineData(3, 10, false)]
    [InlineData(1, 7, true)]
    public void AppliesTo_ReturnsWhetherNumberIsAMultiple(int divisor, int number, bool expected)
    {
        var rule = new FizzBuzzRule(divisor, "x");

        Assert.Equal(expected, rule.AppliesTo(number));
    }
}
