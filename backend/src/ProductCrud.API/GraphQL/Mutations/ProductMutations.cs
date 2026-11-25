using HotChocolate;
using ProductCrud.API.GraphQL.Inputs;
using ProductCrud.Application.Products.DTOs;
using ProductCrud.Application.Products.Interfaces;

namespace ProductCrud.API.GraphQL.Mutations;

public class ProductMutations
{
    public async Task<ProductDto> CreateProduct(
        CreateProductInput input,
        [Service] IProductService productService,
        CancellationToken cancellationToken)
        => await productService.CreateAsync(
            new CreateProductRequest(input.Name, input.Description, input.Price, input.Quantity),
            cancellationToken);

    public async Task<ProductDto> UpdateProduct(
        UpdateProductInput input,
        [Service] IProductService productService,
        CancellationToken cancellationToken)
        => await productService.UpdateAsync(
            new UpdateProductRequest(input.Id, input.Name, input.Description, input.Price, input.Quantity),
            cancellationToken);

    public async Task<bool> DeleteProduct(
        int id,
        [Service] IProductService productService,
        CancellationToken cancellationToken)
        => await productService.DeleteAsync(id, cancellationToken);
}
