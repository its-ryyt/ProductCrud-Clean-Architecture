using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductCrud.Application.Common.Exceptions;
using ProductCrud.Domain.Exceptions;

namespace ProductCrud.API.Middleware;

/// <summary>
/// Global safety net. Catches any unhandled exception in the HTTP pipeline and
/// returns a consistent JSON error instead of an empty 500 / HTML error page.
///
/// Note: exceptions thrown inside GraphQL resolvers are handled by Hot Chocolate
/// and GraphQLErrorFilter. This middleware covers everything outside of that.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected; nobody is waiting for a response.
            _logger.LogInformation("Request was cancelled by the client.");
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                // Too late to change the status code or body.
                _logger.LogError(ex, "Unhandled exception after the response started.");
                throw;
            }

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        var (statusCode, code, message) = ex switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "VALIDATION_ERROR", ex.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND", ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
        };

        if (statusCode >= 500)
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        else
            _logger.LogWarning("Handled {Code}: {Message}", code, ex.Message);

        // Show technical details only while developing
        var detail = statusCode >= 500 && _environment.IsDevelopment() ? ex.ToString() : null;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(
            new ErrorResponse(code, message, context.TraceIdentifier, detail));
    }
}
