using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ProductCrud.API.Middleware;

/// <summary>
/// Gives every request an ID so all log lines from one request can be linked.
/// - Reuses the incoming X-Correlation-ID header if the caller sent one.
/// - Otherwise generates a new GUID.
/// - Returns the ID in the response header and adds it to the logging scope.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const int MaxLength = 64;

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        // Used by ExceptionHandlingMiddleware as the traceId in error responses
        context.TraceIdentifier = correlationId;

        // Set before the response starts, so the header is always sent
        context.Response.Headers[HeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].ToString();

        // Ignore missing or suspiciously long values (protects the logs)
        return string.IsNullOrWhiteSpace(incoming) || incoming.Length > MaxLength
            ? Guid.NewGuid().ToString()
            : incoming;
    }
}
