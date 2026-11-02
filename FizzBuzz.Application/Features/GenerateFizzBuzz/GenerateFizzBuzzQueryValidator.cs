using FizzBuzz.Domain;
using Microsoft.Extensions.Options;

namespace FizzBuzz.Application.Features.GenerateFizzBuzz;

/// <summary>
/// Checks a <see cref="GenerateFizzBuzzQuery"/> against the domain invariants and the configured limit,
/// reporting every violation at once instead of stopping at the first one.
/// </summary>
public sealed class GenerateFizzBuzzQueryValidator(IOptions<FizzBuzzOptions> options)
{
    private readonly int _maxLimit = options.Value.MaxLimit;

    /// <summary>Returns the validation errors indexed by query property name; empty when the query is valid.</summary>
    public IReadOnlyDictionary<string, string[]> Validate(GenerateFizzBuzzQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var errors = new Dictionary<string, string[]>();

        AddError(errors, nameof(query.Int1), ValidateDivisor(query.Int1));
        AddError(errors, nameof(query.Int2), ValidateDivisor(query.Int2));
        AddError(errors, nameof(query.Limit), ValidateLimit(query.Limit));
        AddError(errors, nameof(query.Str1), ValidateReplacement(query.Str1));
        AddError(errors, nameof(query.Str2), ValidateReplacement(query.Str2));

        return errors;
    }

    private static string? ValidateDivisor(int divisor) =>
        divisor > 0 ? null : "Must be greater than 0.";

    private string? ValidateLimit(int limit) =>
        limit >= 1 && limit <= _maxLimit ? null : $"Must be between 1 and {_maxLimit}.";

    private static string? ValidateReplacement(string? replacement)
    {
        if (string.IsNullOrWhiteSpace(replacement))
        {
            return "Must not be empty.";
        }

        return FizzBuzzRule.CountCharacters(replacement) <= FizzBuzzRule.MaxReplacementLength
            ? null
            : $"Must not exceed {FizzBuzzRule.MaxReplacementLength} characters.";
    }

    private static void AddError(Dictionary<string, string[]> errors, string key, string? error)
    {
        if (error is not null)
        {
            errors[key] = [error];
        }
    }
}
