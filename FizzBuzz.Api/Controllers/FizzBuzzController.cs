using System.Net.Mime;
using System.Text.Json;
using FizzBuzz.Api.Contracts;
using FizzBuzz.Api.RateLimiting;
using FizzBuzz.Application.Features.GenerateFizzBuzz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FizzBuzz.Api.Controllers;

/// <summary>FizzBuzz sequence generation.</summary>
[ApiController]
[Route("api/v1/fizzbuzz")]
public sealed partial class FizzBuzzController(
    IGenerateFizzBuzzUseCase generateFizzBuzz,
    ILogger<FizzBuzzController> logger) : ControllerBase
{
    /// <summary>Generates the FizzBuzz sequence from 1 to <c>limit</c>.</summary>
    /// <remarks>
    /// Multiples of <c>int1</c> are replaced by <c>str1</c>, multiples of <c>int2</c> by <c>str2</c>,
    /// multiples of both by <c>str1</c> followed by <c>str2</c>; other numbers are returned as is.
    /// </remarks>
    /// <response code="200">The sequence, in ascending order.</response>
    /// <response code="400">A parameter is missing, malformed or out of range.</response>
    /// <response code="429">Too many requests from the same client.</response>
    [HttpGet]
    [EnableRateLimiting(RateLimitingSetup.FizzBuzzPolicy)]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests, MediaTypeNames.Application.ProblemJson)]
    public ActionResult<IReadOnlyList<string>> Generate([FromQuery] FizzBuzzRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = generateFizzBuzz.Execute(request.ToQuery());
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        // Report application errors under the query string names (Int1 -> int1), like binding errors.
        var invalidParameters = new List<string>(result.Errors.Count);
        foreach (var (property, messages) in result.Errors)
        {
            var parameter = JsonNamingPolicy.CamelCase.ConvertName(property);
            invalidParameters.Add(parameter);
            foreach (var message in messages)
            {
                ModelState.AddModelError(parameter, message);
            }
        }

        LogRequestRejected(logger, invalidParameters);
        return ValidationProblem(ModelState);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "FizzBuzz request rejected, invalid parameters: {InvalidParameters}")]
    private static partial void LogRequestRejected(ILogger logger, IEnumerable<string> invalidParameters);
}
