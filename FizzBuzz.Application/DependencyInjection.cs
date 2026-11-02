using FizzBuzz.Application.Features.GenerateFizzBuzz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FizzBuzz.Application;

/// <summary>Registers the application layer services.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the use cases and their options. Every service is stateless and thread-safe,
    /// and only depends on singletons (<c>IOptions</c>), hence the singleton lifetime:
    /// no per-request allocation and no captive dependency.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<FizzBuzzOptions>()
            .Bind(configuration.GetSection(FizzBuzzOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<GenerateFizzBuzzQueryValidator>();
        services.AddSingleton<IGenerateFizzBuzzUseCase, GenerateFizzBuzzUseCase>();

        return services;
    }
}
