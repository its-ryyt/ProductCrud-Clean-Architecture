namespace ProductCrud.Application.Products.DTOs;

/// <summary>Data needed to create a product.</summary>
public record CreateProductRequest(string Name, string? Description, decimal Price, int Quantity);

/// <summary>Data needed to update a product.</summary>
public record UpdateProductRequest(int Id, string Name, string? Description, decimal Price, int Quantity);
