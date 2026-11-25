using HotChocolate;
using ProductCrud.Application.Products.DTOs;
using ProductCrud.Application.Products.Interfaces;

namespace ProductCrud.API.GraphQL.Queries;

public class ProductQueries
{
    // query { products { id name description price quantity createdAt updatedAt } }
    public async Task<IReadOnlyList<ProductDto>> GetProducts(
        [Service] IProductService productService,
        CancellationToken cancellationToken)
        => await productService.GetAllAsync(cancellationToken);

    // query { product(id: 1) { id name } }
    public async Task<ProductDto?> GetProduct(
        int id,
        [Service] IProductService productService,
        CancellationToken cancellationToken)
        => await productService.GetByIdAsync(id, cancellationToken);
}
