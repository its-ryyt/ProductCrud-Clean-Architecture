using ProductCrud.Application.Common.Exceptions;
using ProductCrud.Application.Common.Interfaces;
using ProductCrud.Application.Products.DTOs;
using ProductCrud.Application.Products.Interfaces;
using ProductCrud.Application.Products.Mappings;
using ProductCrud.Domain.Entities;

namespace ProductCrud.Application.Products.Services;

/// <summary>
/// Orchestrates the product use cases. Business rules live in the Product entity;
/// persistence is delegated to IProductRepository.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        return products.Select(p => p.ToDto()).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product?.ToDto();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(request.Name, request.Description, request.Price, request.Quantity);
        await _repository.AddAsync(product, cancellationToken);
        return product.ToDto();
    }

    public async Task<ProductDto> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.Update(request.Name, request.Description, request.Price, request.Quantity);
        await _repository.UpdateAsync(product, cancellationToken);
        return product.ToDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        await _repository.DeleteAsync(product, cancellationToken);
        return true;
    }
}
