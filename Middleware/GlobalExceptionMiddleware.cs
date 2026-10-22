using System.Net;
using System.Text.Json;

namespace DigitalFormsSystem.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred.");

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, errorMessage) = exception switch
        {
            ArgumentException => ((int)HttpStatusCode.BadRequest, exception.Message),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, exception.Message),
            InvalidOperationException => ((int)HttpStatusCode.Conflict, exception.Message),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized access."),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        // Prevent overriding the status code if it was already set (e.g., by a validation filter)
        if (!response.HasStarted)
        {
            response.StatusCode = statusCode;
        }

        var errorResponse = new
        {
            success = false,
            statusCode = response.StatusCode,
            message = errorMessage,
            timestamp = DateTime.UtcNow
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}