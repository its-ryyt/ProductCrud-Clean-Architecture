namespace ProductCrud.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule of the domain is broken (e.g. negative price).
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
