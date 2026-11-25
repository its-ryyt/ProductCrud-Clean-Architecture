using ProductCrud.Domain.Exceptions;

namespace ProductCrud.Domain.Entities;

/// <summary>
/// Product entity. Its state can only change through Create() and Update(),
/// so a Product can never exist in an invalid state.
/// </summary>
public class Product
{
    public const int NameMaxLength = 150;
    public const int DescriptionMaxLength = 500;

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Required by EF Core
    private Product() { }

    public static Product Create(string name, string? description, decimal price, int quantity)
    {
        var product = new Product { CreatedAt = DateTime.UtcNow };
        product.SetDetails(name, description, price, quantity);
        return product;
    }

    public void Update(string name, string? description, decimal price, int quantity)
    {
        SetDetails(name, description, price, quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    // All checks run BEFORE any value is assigned, so a failed update leaves the product unchanged.
    private void SetDetails(string name, string? description, decimal price, int quantity)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (trimmedName.Length == 0)
            throw new DomainException("Product name is required.");
        if (trimmedName.Length > NameMaxLength)
            throw new DomainException($"Product name must not exceed {NameMaxLength} characters.");
        if (trimmedDescription is not null && trimmedDescription.Length > DescriptionMaxLength)
            throw new DomainException($"Description must not exceed {DescriptionMaxLength} characters.");
        if (price < 0)
            throw new DomainException("Price must not be negative.");
        if (quantity < 0)
            throw new DomainException("Quantity must not be negative.");

        Name = trimmedName;
        Description = trimmedDescription;
        Price = price;
        Quantity = quantity;
    }
}
