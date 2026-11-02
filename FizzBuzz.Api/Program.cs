using System.Diagnostics;
using FizzBuzz.Api.ErrorHandling;
using FizzBuzz.Api.HealthChecks;
using FizzBuzz.Api.RateLimiting;
using FizzBuzz.Application;
using Microsoft.AspNetCore.HttpLogging;

const string TraceIdHeader = "X-Trace-Id";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);

// Application layer (use cases are stateless singletons, see DependencyInjection.AddApplication).
builder.Services.AddApplication(builder.Configuration);

// Presentation layer
builder.Services.AddControllers(options =>
{
    // The error key already names the parameter; never echo the raw input back.
    const string InvalidNumber = "This parameter must be a valid integer.";
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((_, _) => InvalidNumber);
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(_ => InvalidNumber);
});
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info.Title = "FizzBuzz API";
    document.Info.Description = "Generates customizable FizzBuzz sequences.";
    return Task.CompletedTask;
}));

// Errors: RFC 9457 ProblemDetails everywhere, enriched with the trace identifier.
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions.TryAdd(
        "traceId", Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Observability: one log line per request with its duration; query strings and bodies are not logged.
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

builder.Services.AddFizzBuzzHealthChecks();
builder.Services.AddFizzBuzzRateLimiting(builder.Configuration);

// CORS is only needed when the frontend is served from another origin without a proxy.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .WithMethods(HttpMethods.Get)
    .AllowAnyHeader()
    .WithExposedHeaders(TraceIdHeader)));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Disabled in containers where TLS is terminated by the reverse proxy / ingress.
if (app.Configuration.GetValue("Https:RedirectionEnabled", defaultValue: true))
{
    app.UseHttpsRedirection();
}

// Exposes the W3C trace id so that a client can quote it when reporting a problem.
app.Use((context, next) =>
{
    context.Response.Headers[TraceIdHeader] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    return next(context);
});

// Health probes run every few seconds: keep them out of the request log.
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"), branch => branch.UseHttpLogging());

app.UseCors();
app.UseRateLimiter();

app.MapControllers();
app.MapFizzBuzzHealthChecks();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.RunAsync();
