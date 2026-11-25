using Microsoft.EntityFrameworkCore;
using ProductCrud.Domain.Entities;
using ProductCrud.Infrastructure.Persistence;
using ProductCrud.Infrastructure.Persistence.Repositories;

namespace ProductCrud.Infrastructure.UnitTests.Repositories;

/// <summary>
/// Tests ProductRepository against the EF Core InMemory provider.
/// Each test gets its own database name, so tests never share data.
/// </summary>
public class ProductRepositoryTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddAsync_SavesProductAndGeneratesId()
    {
        var dbName = Guid.NewGuid().ToString();
        var product = Product.Create("Keyboard", null, 2500m, 10);

        await using (var context = CreateContext(dbName))
        {
            await new ProductRepository(context).AddAsync(product);
        }

        Assert.True(product.Id > 0);
        await using var verify = CreateContext(dbName);
        Assert.Equal(1, await verify.Products.CountAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProductsOrderedById()
    {
        var dbName = Guid.NewGuid().ToString();
        await using (var context = CreateContext(dbName))
        {
            var repo = new ProductRepository(context);
            await repo.AddAsync(Product.Create("Mouse", null, 100m, 1));
            await repo.AddAsync(Product.Create("Keyboard", null, 200m, 2));
        }

        await using var readContext = CreateContext(dbName);
        var result = await new ProductRepository(readContext).GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.True(result[0].Id < result[1].Id);
        Assert.Equal("Mouse", result[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        await using var context = CreateContext(Guid.NewGuid().ToString());

        var result = await new ProductRepository(context).GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        int id;
        await using (var context = CreateContext(dbName))
        {
            var product = Product.Create("Keyboard", null, 2500m, 10);
            await new ProductRepository(context).AddAsync(product);
            id = product.Id;
        }

        await using (var context = CreateContext(dbName))
        {
            var repo = new ProductRepository(context);
            var product = await repo.GetByIdAsync(id);
            product!.Update("Keyboard Pro", "RGB", 3000m, 5);
            await repo.UpdateAsync(product);
        }

        await using var verify = CreateContext(dbName);
        var saved = await verify.Products.SingleAsync(p => p.Id == id);
        Assert.Equal("Keyboard Pro", saved.Name);
        Assert.Equal(3000m, saved.Price);
        Assert.NotNull(saved.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var dbName = Guid.NewGuid().ToString();
        int id;
        await using (var context = CreateContext(dbName))
        {
            var product = Product.Create("Mouse", null, 100m, 1);
            await new ProductRepository(context).AddAsync(product);
            id = product.Id;
        }

        await using (var context = CreateContext(dbName))
        {
            var repo = new ProductRepository(context);
            var product = await repo.GetByIdAsync(id);
            await repo.DeleteAsync(product!);
        }

        await using var verify = CreateContext(dbName);
        Assert.False(await verify.Products.AnyAsync());
    }
}
