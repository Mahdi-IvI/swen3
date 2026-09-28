using Bll.@new.Exceptions;
using Dal;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Exceptions;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    private readonly ILogger _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred while processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request failed with status {StatusCode} for {Method} {Path}",
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Detail = ShouldExposeDetails(statusCode)
                ? exception.Message
                : "An unexpected error occurred.",
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["exception"] = exception.GetType().Name;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private bool ShouldExposeDetails(int statusCode)
    {
        return statusCode < StatusCodes.Status500InternalServerError || environment.IsDevelopment();
    }

    private static (int StatusCode, string Title) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            FormatException => (StatusCodes.Status400BadRequest, "Invalid request format"),
            UserNotFoundException
                => (StatusCodes.Status404NotFound, "Resource not found"),
            UserAlreadyExistsException
                or DuplicateKeyException
                => (StatusCodes.Status409Conflict, "Resource conflict"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access denied"),
            NotImplementedException => (StatusCodes.Status501NotImplemented, "Feature not implemented"),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error")
        };
    }
}
