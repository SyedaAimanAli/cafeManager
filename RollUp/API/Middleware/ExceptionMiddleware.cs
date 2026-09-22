using System.Net;
using System.Text.Json;

namespace RollUp.API.Middleware;

/// <summary>
/// Global exception handler — catches unhandled exceptions and returns a
/// consistent JSON problem response instead of leaking stack traces.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                status  = 500,
                message = "An unexpected error occurred.",
                detail  = _env.IsDevelopment() ? ex.Message : null
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
