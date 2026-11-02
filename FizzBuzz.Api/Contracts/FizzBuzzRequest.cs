using System.ComponentModel.DataAnnotations;
using FizzBuzz.Application.Features.GenerateFizzBuzz;
using Microsoft.AspNetCore.Mvc;

namespace FizzBuzz.Api.Contracts;

/// <summary>
/// Query string of <c>GET /api/v1/fizzbuzz</c>. Only presence and format are checked here
/// (missing or non-numeric values); business rules are validated by the application layer.
/// </summary>
public sealed class FizzBuzzRequest
{
    internal const string RequiredMessage = "This parameter is required.";

    /// <summary>First divisor, strictly positive.</summary>
    /// <example>3</example>
    [Required(ErrorMessage = RequiredMessage)]
    [FromQuery(Name = "int1")]
    public int? Int1 { get; init; }

    /// <summary>Second divisor, strictly positive.</summary>
    /// <example>5</example>
    [Required(ErrorMessage = RequiredMessage)]
    [FromQuery(Name = "int2")]
    public int? Int2 { get; init; }

    /// <summary>Inclusive upper bound, between 1 and the configured maximum (10 000 at most).</summary>
    /// <example>15</example>
    [Required(ErrorMessage = RequiredMessage)]
    [FromQuery(Name = "limit")]
    public int? Limit { get; init; }

    /// <summary>Replacement for multiples of <c>int1</c>, 1 to 50 characters.</summary>
    /// <example>Fizz</example>
    [Required(ErrorMessage = RequiredMessage)]
    [FromQuery(Name = "str1")]
    public string? Str1 { get; init; }

    /// <summary>Replacement for multiples of <c>int2</c>, 1 to 50 characters.</summary>
    /// <example>Buzz</example>
    [Required(ErrorMessage = RequiredMessage)]
    [FromQuery(Name = "str2")]
    public string? Str2 { get; init; }

    /// <summary>Maps the request to the use case input. Only called once model validation succeeded.</summary>
    internal GenerateFizzBuzzQuery ToQuery() => new(
        Int1.GetValueOrDefault(),
        Int2.GetValueOrDefault(),
        Limit.GetValueOrDefault(),
        Str1 ?? string.Empty,
        Str2 ?? string.Empty);
}
