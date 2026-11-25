using Microsoft.EntityFrameworkCore;
using ProductCrud.Domain.Entities;

namespace ProductCrud.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Loads every IEntityTypeConfiguration in this assembly (e.g. ProductConfiguration)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
