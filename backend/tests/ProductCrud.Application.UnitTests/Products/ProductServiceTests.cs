using Moq;
using ProductCrud.Application.Common.Exceptions;
using ProductCrud.Application.Common.Interfaces;
using ProductCrud.Application.Products.DTOs;
using ProductCrud.Application.Products.Services;
using ProductCrud.Domain.Entities;
using ProductCrud.Domain.Exceptions;

namespace ProductCrud.Application.UnitTests.Products;

/// <summary>
/// Tests the product use cases in isolation.
/// IProductRepository is replaced with a Moq mock, so no database is needed.
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repository = new();
    private readonly ProductService _sut; // sut = "system under test"

    public ProductServiceTests()
    {
        _sut = new ProductService(_repository.Object);
    }

    // ---------- GetAll ----------

    [Fact]
    public async Task GetAllAsync_ReturnsMappedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            Product.Create("Mouse", "Wireless", 499.50m, 20),
            Product.Create("Keyboard", null, 2500m, 10)
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Mouse", result[0].Name);
        Assert.Equal(499.50m, result[0].Price);
        Assert.Equal("Keyboard", result[1].Name);
        Assert.Null(result[1].Description);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
    {
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<Product>());

        var result = await _sut.GetAllAsync();

        Assert.Empty(result);
    }

    // ---------- GetById ----------

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsDto()
    {
        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Product.Create("Mouse", null, 100m, 1));

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Mouse", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.Null(result);
    }

    // ---------- Create ----------

    [Fact]
    public async Task CreateAsync_WithValidRequest_AddsProductAndReturnsDto()
    {
        var request = new CreateProductRequest("  Keyboard  ", "Mechanical", 2500m, 10);

        var result = await _sut.CreateAsync(request);

        Assert.Equal("Keyboard", result.Name);
        Assert.Equal(2500m, result.Price);
        _repository.Verify(r => r.AddAsync(
                It.Is<Product>(p => p.Name == "Keyboard" && p.Quantity == 10),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRequest_ThrowsAndDoesNotSave()
    {
        var request = new CreateProductRequest("", null, 100m, 1);

        await Assert.ThrowsAsync<DomainException>(() => _sut.CreateAsync(request));

        _repository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- Update ----------

    [Fact]
    public async Task UpdateAsync_WhenFound_UpdatesAndSaves()
    {
        var existing = Product.Create("Keyboard", null, 2500m, 10);
        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(existing);

        var result = await _sut.UpdateAsync(new UpdateProductRequest(1, "Keyboard Pro", "RGB", 3000m, 5));

        Assert.Equal("Keyboard Pro", result.Name);
        Assert.Equal(3000m, result.Price);
        Assert.NotNull(result.UpdatedAt);
        _repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Product?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateAsync(new UpdateProductRequest(99, "X", null, 1m, 1)));

        Assert.Equal("Product with id 99 was not found.", ex.Message);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidData_ThrowsAndDoesNotSave()
    {
        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Product.Create("Keyboard", null, 2500m, 10));

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateAsync(new UpdateProductRequest(1, "Keyboard", null, -1m, 10)));

        _repository.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- Delete ----------

    [Fact]
    public async Task DeleteAsync_WhenFound_DeletesAndReturnsTrue()
    {
        var existing = Product.Create("Mouse", null, 100m, 1);
        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(existing);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result);
        _repository.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(99));

        _repository.Verify(r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
