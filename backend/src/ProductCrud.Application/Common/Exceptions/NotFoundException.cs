namespace ProductCrud.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested record does not exist.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id {key} was not found.") { }
}
