using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProductCrud.API.Middleware;

namespace ProductCrud.API.UnitTests.Middleware;

public class RequestLoggingMiddlewareTests
{
    private readonly TestLogger<RequestLoggingMiddleware> _logger = new();

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/graphql";
        return context;
    }

    [Theory]
    [InlineData(200, LogLevel.Information)]
    [InlineData(404, LogLevel.Warning)]
    [InlineData(500, LogLevel.Error)]
    public async Task InvokeAsync_LogsWithLevelBasedOnStatusCode(int statusCode, LogLevel expectedLevel)
    {
        // Arrange
        var context = CreateContext();
        var middleware = new RequestLoggingMiddleware(ctx =>
        {
            ctx.Response.StatusCode = statusCode;
            return Task.CompletedTask;
        }, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(expectedLevel, entry.Level);
        Assert.Contains("POST /graphql", entry.Message);
        Assert.Contains($"responded {statusCode}", entry.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrows_LogsErrorAndRethrows()
    {
        var context = CreateContext();
        var middleware = new RequestLoggingMiddleware(_ => throw new InvalidOperationException("boom"), _logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("responded 500", entry.Message);
    }
}
