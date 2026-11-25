using HotChocolate;
using ProductCrud.Application.Common.Exceptions;
using ProductCrud.Domain.Exceptions;

namespace ProductCrud.API.GraphQL.Errors;

/// <summary>
/// Converts known exceptions into readable GraphQL errors for the frontend.
/// </summary>
public class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error) => error.Exception switch
    {
        DomainException ex => error
            .WithMessage(ex.Message)
            .WithCode("VALIDATION_ERROR")
            .RemoveException(),

        NotFoundException ex => error
            .WithMessage(ex.Message)
            .WithCode("NOT_FOUND")
            .RemoveException(),

        _ => error
    };
}
