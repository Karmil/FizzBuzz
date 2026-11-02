namespace FizzBuzz.Application.Common;

/// <summary>
/// Outcome of a use case: either a value or a set of validation errors.
/// Invalid input is an expected outcome, so it is returned rather than thrown.
/// </summary>
/// <typeparam name="T">Type of the value produced on success.</typeparam>
public sealed class Result<T>
{
    private readonly T? _value;

    internal Result(T? value, IReadOnlyDictionary<string, string[]> errors)
    {
        _value = value;
        Errors = errors;
    }

    /// <summary>Indicates whether the use case succeeded.</summary>
    public bool IsSuccess => Errors.Count == 0;

    /// <summary>Validation errors indexed by input name; empty on success.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>Value produced by the use case.</summary>
    /// <exception cref="InvalidOperationException">The result is not a success.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("An invalid result has no value.");
}

/// <summary>Factory methods for <see cref="Result{T}"/>.</summary>
public static class Result
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors = new Dictionary<string, string[]>();

    /// <summary>Creates a successful result.</summary>
    public static Result<T> Success<T>(T value) => new(value, NoErrors);

    /// <summary>Creates a result carrying at least one validation error.</summary>
    /// <exception cref="ArgumentException"><paramref name="errors"/> is empty.</exception>
    public static Result<T> Invalid<T>(IReadOnlyDictionary<string, string[]> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
        {
            throw new ArgumentException("An invalid result requires at least one error.", nameof(errors));
        }

        return new(default, errors);
    }
}
