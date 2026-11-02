using System.ComponentModel.DataAnnotations;
using FizzBuzz.Domain;

namespace FizzBuzz.Application;

/// <summary>Configurable settings of the FizzBuzz feature, bound to the <c>FizzBuzz</c> configuration section.</summary>
public sealed class FizzBuzzOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "FizzBuzz";

    /// <summary>
    /// Maximum accepted <c>limit</c>. It may be lowered to protect the service,
    /// but never above <see cref="FizzBuzzGenerator.MaxLimit"/> (checked at startup).
    /// </summary>
    [Range(1, FizzBuzzGenerator.MaxLimit)]
    public int MaxLimit { get; set; } = FizzBuzzGenerator.MaxLimit;
}
