using System.Text.Json.Serialization;

namespace ProductCrud.API.Middleware;

/// <summary>
/// JSON body returned by ExceptionHandlingMiddleware.
/// Example: { "code": "INTERNAL_ERROR", "message": "An unexpected error occurred.", "traceId": "..." }
/// </summary>
public record ErrorResponse(
    string Code,
    string Message,
    string TraceId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Detail = null);
