using ProductCrud.Domain.Entities;

namespace ProductCrud.Application.Common.Interfaces;

/// <summary>
/// Persistence contract. Defined here (Application) and implemented in Infrastructure
/// so the Application layer never depends on EF Core or SQL Server.
/// </summary>
public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
}
