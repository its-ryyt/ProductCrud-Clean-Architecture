namespace ProductCrud.Application.Products.DTOs;

/// <summary>
/// Data returned to the outside world (GraphQL). Never expose the entity directly.
/// </summary>
public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
