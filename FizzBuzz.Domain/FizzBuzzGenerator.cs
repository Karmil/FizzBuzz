using System.Globalization;

namespace FizzBuzz.Domain;

/// <summary>
/// Produces the FizzBuzz sequence. Pure and deterministic: the same inputs always yield the same output.
/// </summary>
public static class FizzBuzzGenerator
{
    /// <summary>Absolute upper bound of <c>limit</c>; configuration may lower it but never raise it.</summary>
    public const int MaxLimit = 10_000;

    /// <summary>
    /// Builds the sequence for every number from 1 to <paramref name="limit"/> inclusive, in ascending order.
    /// A multiple of both divisors yields the first replacement followed by the second one;
    /// a number matching no rule yields its invariant-culture decimal representation.
    /// </summary>
    /// <exception cref="ArgumentNullException">A rule is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="limit"/> is outside [1, <see cref="MaxLimit"/>].</exception>
    public static IReadOnlyList<string> Generate(FizzBuzzRule first, FizzBuzzRule second, int limit)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, MaxLimit);

        var combined = first.Replacement + second.Replacement;
        var values = new string[limit];

        for (var number = 1; number <= limit; number++)
        {
            values[number - 1] = (first.AppliesTo(number), second.AppliesTo(number)) switch
            {
                (true, true) => combined,
                (true, false) => first.Replacement,
                (false, true) => second.Replacement,
                _ => number.ToString(CultureInfo.InvariantCulture),
            };
        }

        return values;
    }
}
