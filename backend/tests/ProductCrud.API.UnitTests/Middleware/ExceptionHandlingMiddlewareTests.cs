using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using ProductCrud.API.Middleware;
using ProductCrud.Application.Common.Exceptions;
using ProductCrud.Domain.Exceptions;

namespace ProductCrud.API.UnitTests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly TestLogger<ExceptionHandlingMiddleware> _logger = new();

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext { TraceIdentifier = "trace-1" };
        context.Response.Body = new MemoryStream();
        return context;
    }

    private ExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next, string environment = "Production")
        => new(next, _logger, new FakeHostEnvironment(environment));

    private static async Task<JsonElement> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);
        return doc.RootElement.Clone();
    }

    [Fact]
    public async Task InvokeAsync_WithoutException_PassesThrough()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task InvokeAsync_DomainException_Returns400ValidationError()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(_ => throw new DomainException("Product name is required."));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("code").GetString());
        Assert.Equal("Product name is required.", body.GetProperty("message").GetString());
        Assert.Equal("trace-1", body.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(_ => throw new NotFoundException("Product", 99));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Equal("NOT_FOUND", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task InvokeAsync_UnknownException_InProduction_HidesDetails()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("secret connection info"));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Equal("INTERNAL_ERROR", body.GetProperty("code").GetString());
        Assert.Equal("An unexpected error occurred.", body.GetProperty("message").GetString());
        Assert.False(body.TryGetProperty("detail", out _));
    }

    [Fact]
    public async Task InvokeAsync_UnknownException_InDevelopment_IncludesDetails()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("boom"), Environments.Development);

        await middleware.InvokeAsync(context);

        var body = await ReadBodyAsync(context);
        Assert.Contains("boom", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task InvokeAsync_ClientCancelled_DoesNotWriteError()
    {
        var context = CreateContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        context.RequestAborted = cts.Token;
        var middleware = CreateMiddleware(_ => throw new OperationCanceledException());

        await middleware.InvokeAsync(context);

        Assert.Equal(0, context.Response.Body.Length);
    }
}
