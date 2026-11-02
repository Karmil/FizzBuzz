using FizzBuzz.Application.Common;
using FizzBuzz.Domain;

namespace FizzBuzz.Application.Features.GenerateFizzBuzz;

/// <inheritdoc />
public sealed class GenerateFizzBuzzUseCase(GenerateFizzBuzzQueryValidator validator) : IGenerateFizzBuzzUseCase
{
    /// <inheritdoc />
    public Result<IReadOnlyList<string>> Execute(GenerateFizzBuzzQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var errors = validator.Validate(query);
        if (errors.Count > 0)
        {
            return Result.Invalid<IReadOnlyList<string>>(errors);
        }

        var values = FizzBuzzGenerator.Generate(
            new FizzBuzzRule(query.Int1, query.Str1),
            new FizzBuzzRule(query.Int2, query.Str2),
            query.Limit);

        return Result.Success(values);
    }
}
