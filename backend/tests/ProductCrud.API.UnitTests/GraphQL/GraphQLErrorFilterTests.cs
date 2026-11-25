using HotChocolate;
using ProductCrud.API.GraphQL.Errors;
using ProductCrud.Application.Common.Exceptions;
using ProductCrud.Domain.Exceptions;

namespace ProductCrud.API.UnitTests.GraphQL;

/// <summary>
/// Tests that exceptions are turned into readable GraphQL errors for the frontend.
/// </summary>
public class GraphQLErrorFilterTests
{
    private readonly GraphQLErrorFilter _sut = new();

    private static IError ErrorFrom(Exception ex) =>
        ErrorBuilder.New()
            .SetMessage("Unexpected Execution Error")
            .SetException(ex)
            .Build();

    [Fact]
    public void OnError_DomainException_ReturnsValidationError()
    {
        var result = _sut.OnError(ErrorFrom(new DomainException("Product name is required.")));

        Assert.Equal("Product name is required.", result.Message);
        Assert.Equal("VALIDATION_ERROR", result.Code);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void OnError_NotFoundException_ReturnsNotFound()
    {
        var result = _sut.OnError(ErrorFrom(new NotFoundException("Product", 99)));

        Assert.Equal("Product with id 99 was not found.", result.Message);
        Assert.Equal("NOT_FOUND", result.Code);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void OnError_UnknownException_IsNotChanged()
    {
        var original = ErrorFrom(new InvalidOperationException("boom"));

        var result = _sut.OnError(original);

        Assert.Same(original, result);
    }
}
