namespace FizzBuzz.Application.Features.GenerateFizzBuzz;

/// <summary>Input of the FizzBuzz generation use case.</summary>
/// <param name="Int1">First divisor.</param>
/// <param name="Int2">Second divisor.</param>
/// <param name="Limit">Inclusive upper bound of the sequence.</param>
/// <param name="Str1">Replacement for multiples of <paramref name="Int1"/>.</param>
/// <param name="Str2">Replacement for multiples of <paramref name="Int2"/>.</param>
public sealed record GenerateFizzBuzzQuery(int Int1, int Int2, int Limit, string Str1, string Str2);
