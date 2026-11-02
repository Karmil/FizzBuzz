using FizzBuzz.Application.Common;

namespace FizzBuzz.Application.Features.GenerateFizzBuzz;

/// <summary>Generates a FizzBuzz sequence from user-supplied parameters.</summary>
public interface IGenerateFizzBuzzUseCase
{
    /// <summary>
    /// Validates <paramref name="query"/> and, when valid, returns the values for 1 to <c>Limit</c> in ascending order.
    /// </summary>
    /// <returns>The sequence, or the validation errors indexed by query property name.</returns>
    Result<IReadOnlyList<string>> Execute(GenerateFizzBuzzQuery query);
}
