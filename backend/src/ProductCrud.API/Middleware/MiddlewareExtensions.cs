using Microsoft.AspNetCore.Builder;

namespace ProductCrud.API.Middleware;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Registers the custom middleware in the correct order.
    /// 1. CorrelationId   - must be first so every log line has the ID
    /// 2. RequestLogging  - wraps everything below, so it logs the final status code
    /// 3. ExceptionHandling - converts unhandled exceptions into a JSON response
    /// </summary>
    public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
