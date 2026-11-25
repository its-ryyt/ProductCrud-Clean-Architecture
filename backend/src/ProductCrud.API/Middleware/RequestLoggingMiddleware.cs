using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ProductCrud.API.Middleware;

/// <summary>
/// Logs one line per request: method, path, status code and duration.
/// Log level: 2xx/3xx = Information, 4xx = Warning, 5xx or exception = Error.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var failed = false;

        try
        {
            await _next(context);
        }
        catch
        {
            failed = true;
            throw; // let the exception continue; we only log it
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;

            var level = failed || statusCode >= 500 ? LogLevel.Error
                : statusCode >= 400 ? LogLevel.Warning
                : LogLevel.Information;

            _logger.Log(level,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                context.Request.Method,
                context.Request.Path.Value,
                failed ? 500 : statusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
