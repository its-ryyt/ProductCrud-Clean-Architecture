using ProductCrud.Application.Products.DTOs;
using ProductCrud.Domain.Entities;

namespace ProductCrud.Application.Products.Mappings;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Quantity = product.Quantity,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}
