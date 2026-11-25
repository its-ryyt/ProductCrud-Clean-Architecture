using Microsoft.AspNetCore.Http;
using ProductCrud.API.Middleware;

namespace ProductCrud.API.UnitTests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly TestLogger<CorrelationIdMiddleware> _logger = new();

    [Fact]
    public async Task InvokeAsync_WithoutHeader_GeneratesGuidAndSetsResponseHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var id = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        Assert.True(Guid.TryParse(id, out _));
        Assert.Equal(id, context.TraceIdentifier);
    }

    [Fact]
    public async Task InvokeAsync_WithHeader_ReusesIncomingId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "abc-123";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger);

        await middleware.InvokeAsync(context);

        Assert.Equal("abc-123", context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
        Assert.Equal("abc-123", context.TraceIdentifier);
    }

    [Fact]
    public async Task InvokeAsync_WithTooLongHeader_GeneratesNewId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] =
            new string('x', CorrelationIdMiddleware.MaxLength + 1);
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger);

        await middleware.InvokeAsync(context);

        Assert.True(Guid.TryParse(context.TraceIdentifier, out _));
    }

    [Fact]
    public async Task InvokeAsync_CallsNextAndAddsLoggingScope()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, _logger);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        var scope = Assert.Single(_logger.Scopes);
        Assert.Equal(context.TraceIdentifier, scope["CorrelationId"]);
    }
}
