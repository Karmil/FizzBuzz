namespace FizzBuzz.Domain;

/// <summary>
/// Associates a divisor with the text that replaces each of its multiples.
/// Instances are immutable and always valid.
/// </summary>
public sealed class FizzBuzzRule
{
    /// <summary>Maximum length of a replacement, counted in Unicode characters (code points).</summary>
    public const int MaxReplacementLength = 50;

    /// <summary>Creates a rule.</summary>
    /// <param name="divisor">Strictly positive divisor.</param>
    /// <param name="replacement">Non-blank text of at most <see cref="MaxReplacementLength"/> characters.</param>
    /// <exception cref="ArgumentOutOfRangeException">The divisor is not strictly positive.</exception>
    /// <exception cref="ArgumentException">The replacement is blank or too long.</exception>
    public FizzBuzzRule(int divisor, string replacement)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(divisor);
        ArgumentException.ThrowIfNullOrWhiteSpace(replacement);

        if (CountCharacters(replacement) > MaxReplacementLength)
        {
            throw new ArgumentException(
                $"The replacement must not exceed {MaxReplacementLength} characters.", nameof(replacement));
        }

        Divisor = divisor;
        Replacement = replacement;
    }

    /// <summary>Divisor whose multiples are replaced.</summary>
    public int Divisor { get; }

    /// <summary>Text emitted for the multiples of <see cref="Divisor"/>.</summary>
    public string Replacement { get; }

    /// <summary>Indicates whether <paramref name="number"/> is a multiple of <see cref="Divisor"/>.</summary>
    public bool AppliesTo(int number) => number % Divisor == 0;

    /// <summary>
    /// Counts user-visible characters as Unicode code points, so that "😀" counts as one character
    /// although it uses two UTF-16 code units.
    /// </summary>
    public static int CountCharacters(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return text.EnumerateRunes().Count();
    }
}
