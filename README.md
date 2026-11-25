# ProductCrud

A simple Product CRUD application built with:

| Layer | Technology |
|---|---|
| Frontend | Next.js (App Router) + React + TypeScript + Tailwind CSS + Apollo Client |
| API | GraphQL (Hot Chocolate 13) |
| Backend | ASP.NET Core Web API (.NET 8) – **Clean Architecture** |
| Data access | Entity Framework Core 8 |
| Database | SQL Server |
| Testing | xUnit + Moq + EF Core InMemory (TDD) |

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Solution Structure](#2-solution-structure)
3. [Backend Setup](#3-backend-setup)
4. [Backend Code](#4-backend-code)
5. [Middleware](#5-middleware)
6. [Database Migration](#6-database-migration)
7. [Run and Test the Backend](#7-run-and-test-the-backend)
8. [Unit Tests and TDD](#8-unit-tests-and-tdd)
9. [Frontend Setup](#9-frontend-setup)
10. [Frontend Code](#10-frontend-code)
11. [Run the Full Application](#11-run-the-full-application)
12. [Adding a New Module](#12-adding-a-new-module)
13. [Troubleshooting](#13-troubleshooting)

---

## 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18.18+ (LTS recommended)](https://nodejs.org/)
- SQL Server (LocalDB, Express, or Developer) + SSMS (optional)
- Visual Studio 2022 or VS Code
- EF Core CLI tool:

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

---

## 2. Solution Structure

```text
C:\Project\ProductCrud\
├── backend\
│   ├── ProductCrud.sln
│   ├── src\
│   ├── tests\
│   └── docs\
├── frontend\
│   ├── src\
│   ├── .env.local
│   └── package.json
└── README.md
```

### Backend structure (Clean Architecture)

```text
backend/
├── ProductCrud.sln
├── src/
│   ├── ProductCrud.Domain/                     # Enterprise rules (no dependencies)
│   │   ├── Entities/Product.cs
│   │   └── Exceptions/DomainException.cs
│   ├── ProductCrud.Application/                # Use cases (depends on Domain)
│   │   ├── Common/
│   │   │   ├── Exceptions/NotFoundException.cs
│   │   │   └── Interfaces/IProductRepository.cs
│   │   ├── Products/
│   │   │   ├── DTOs/ProductDto.cs
│   │   │   ├── DTOs/ProductRequests.cs
│   │   │   ├── Interfaces/IProductService.cs
│   │   │   ├── Mappings/ProductMappings.cs
│   │   │   └── Services/ProductService.cs
│   │   └── DependencyInjection.cs
│   ├── ProductCrud.Infrastructure/             # EF Core + SQL Server (depends on Application)
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/ProductConfiguration.cs
│   │   │   ├── Repositories/ProductRepository.cs
│   │   │   └── Migrations/
│   │   └── DependencyInjection.cs
│   └── ProductCrud.API/                        # GraphQL presentation (composition root)
│       ├── GraphQL/
│       │   ├── Inputs/ProductInputs.cs
│       │   ├── Queries/ProductQueries.cs
│       │   ├── Mutations/ProductMutations.cs
│       │   └── Errors/GraphQLErrorFilter.cs
│       ├── Middleware/
│       │   ├── CorrelationIdMiddleware.cs
│       │   ├── RequestLoggingMiddleware.cs
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   ├── ErrorResponse.cs
│       │   └── MiddlewareExtensions.cs
│       ├── Properties/launchSettings.json
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
├── tests/
│   ├── ProductCrud.Domain.UnitTests/           # Entities/ProductTests.cs
│   ├── ProductCrud.Application.UnitTests/      # Products/ProductServiceTests.cs
│   ├── ProductCrud.Infrastructure.UnitTests/   # Repositories/ProductRepositoryTests.cs
│   └── ProductCrud.API.UnitTests/              # GraphQL/ and Middleware/ tests
└── docs/
    └── TDD-GUIDE.md
```

### Dependency rule

```text
API ──► Application ──► Domain
 │            ▲
 └──► Infrastructure
```

Dependencies always point **inward**. The Domain knows nothing about EF Core, SQL Server, or GraphQL.

| Project | Purpose | References |
|---|---|---|
| `ProductCrud.Domain` | `Product` entity and its business rules, `DomainException` | None |
| `ProductCrud.Application` | Use cases (`ProductService`), DTOs, `IProductRepository` / `IProductService`, `NotFoundException` | Domain |
| `ProductCrud.Infrastructure` | `AppDbContext`, EF configurations, `ProductRepository`, migrations | Application |
| `ProductCrud.API` | GraphQL queries/mutations, error filter, middleware, CORS, `Program.cs` | Application, Infrastructure |
| `tests/*.UnitTests` | One test project per layer | The layer being tested |

### What changed from the previous layered version

| Before | Now (Clean Architecture) |
|---|---|
| `ProductCrud.Core` (entities + DTOs + interfaces) | Split into **Domain** (entities) and **Application** (DTOs + interfaces) |
| `ProductService` in `Infrastructure` | Moved to **Application** |
| `ProductCrud.Data` (EF Core) | Renamed to **Infrastructure** (`Persistence/` folder) |
| Validation in `ProductService` | Moved into the **`Product` entity** (`Create()` / `Update()`, private setters) |
| `ArgumentException` / `KeyNotFoundException` | `DomainException` / `NotFoundException` |
| No tests | 4 unit test projects + TDD guide |
| No custom middleware | Correlation ID, request logging, global exception handling |

### Frontend structure

```text
frontend/
├── .env.local
└── src/
    ├── app/                              # Routing only (thin pages)
    │   ├── (main)/
    │   │   ├── layout.tsx                # App shell (header/nav)
    │   │   └── products/page.tsx
    │   ├── globals.css
    │   ├── layout.tsx                    # Root layout + providers
    │   └── page.tsx                      # Redirect to /products
    ├── components/
    │   └── ui/                           # Reusable, business-agnostic UI
    │       ├── Alert.tsx
    │       ├── Button.tsx
    │       └── Input.tsx
    ├── config/
    │   └── env.ts                        # Environment variables in one place
    ├── features/
    │   └── products/                     # Product module
    │       ├── api/
    │       │   ├── product.queries.ts
    │       │   └── product.mutations.ts
    │       ├── components/
    │       │   ├── ProductForm.tsx
    │       │   ├── ProductTable.tsx
    │       │   └── ProductsView.tsx      # Page container (state + logic)
    │       ├── hooks/
    │       │   └── useProducts.ts
    │       ├── types/
    │       │   └── product.types.ts
    │       ├── utils/
    │       │   └── product.validation.ts
    │       └── index.ts                  # Public exports of the module
    ├── lib/
    │   ├── apollo/
    │   │   └── apollo-client.ts
    │   └── utils/
    │       ├── error.ts
    │       └── format.ts
    └── providers/
        └── AppProviders.tsx
```

| Folder | Purpose |
|---|---|
| `app/` | Routes and layouts only. Pages just render a feature component |
| `components/ui/` | Generic, reusable UI with no business logic |
| `config/` | Environment variables and app settings |
| `features/<module>/` | Everything for one business module |
| `lib/` | Third-party setup (Apollo) and shared helpers |
| `providers/` | Global React context providers |

---

## 3. Backend Setup

### 3.1 Create the solution and projects

```powershell
cd C:\Project\ProductCrud\backend
dotnet new sln -n ProductCrud

# Source projects
dotnet new classlib -n ProductCrud.Domain         -o src/ProductCrud.Domain         -f net8.0
dotnet new classlib -n ProductCrud.Application    -o src/ProductCrud.Application    -f net8.0
dotnet new classlib -n ProductCrud.Infrastructure -o src/ProductCrud.Infrastructure -f net8.0
dotnet new web      -n ProductCrud.API            -o src/ProductCrud.API            -f net8.0

# Test projects
dotnet new xunit -n ProductCrud.Domain.UnitTests         -o tests/ProductCrud.Domain.UnitTests         -f net8.0
dotnet new xunit -n ProductCrud.Application.UnitTests    -o tests/ProductCrud.Application.UnitTests    -f net8.0
dotnet new xunit -n ProductCrud.Infrastructure.UnitTests -o tests/ProductCrud.Infrastructure.UnitTests -f net8.0
dotnet new xunit -n ProductCrud.API.UnitTests            -o tests/ProductCrud.API.UnitTests            -f net8.0

# Add to solution (in solution folders)
dotnet sln add (Get-ChildItem src   -Recurse -Filter *.csproj) --solution-folder src
dotnet sln add (Get-ChildItem tests -Recurse -Filter *.csproj) --solution-folder tests
```

Delete the auto-generated `Class1.cs` in **Domain**, **Application**, **Infrastructure**, and `UnitTest1.cs` in each test project.

### 3.2 Add project references

```powershell
# Source
dotnet add src/ProductCrud.Application    reference src/ProductCrud.Domain
dotnet add src/ProductCrud.Infrastructure reference src/ProductCrud.Application
dotnet add src/ProductCrud.API            reference src/ProductCrud.Application src/ProductCrud.Infrastructure

# Tests (each test project references only the layer it tests)
dotnet add tests/ProductCrud.Domain.UnitTests         reference src/ProductCrud.Domain
dotnet add tests/ProductCrud.Application.UnitTests    reference src/ProductCrud.Application
dotnet add tests/ProductCrud.Infrastructure.UnitTests reference src/ProductCrud.Infrastructure
dotnet add tests/ProductCrud.API.UnitTests            reference src/ProductCrud.API
```

### 3.3 Install NuGet packages

```powershell
# Application
dotnet add src/ProductCrud.Application package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.2

# Infrastructure
dotnet add src/ProductCrud.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
dotnet add src/ProductCrud.Infrastructure package Microsoft.Extensions.Configuration.Abstractions --version 8.0.0

# API
dotnet add src/ProductCrud.API package HotChocolate.AspNetCore --version 13.9.14
dotnet add src/ProductCrud.API package Microsoft.EntityFrameworkCore.Design --version 8.0.11
dotnet add src/ProductCrud.API package Microsoft.EntityFrameworkCore.Tools --version 8.0.11

# Tests
dotnet add tests/ProductCrud.Application.UnitTests    package Moq --version 4.20.72
dotnet add tests/ProductCrud.Infrastructure.UnitTests package Microsoft.EntityFrameworkCore.InMemory --version 8.0.11
```

The API test project also needs ASP.NET Core types (`HttpContext`) for the middleware tests. Add this to `tests/ProductCrud.API.UnitTests/ProductCrud.API.UnitTests.csproj`:

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

---

## 4. Backend Code

### 4.1 ProductCrud.Domain

#### Exceptions/DomainException.cs

```csharp
namespace ProductCrud.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule of the domain is broken (e.g. negative price).
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

#### Entities/Product.cs

> The setters are **private**. A `Product` can only be created with `Create()` and changed with `Update()`, so it can never be in an invalid state.

```csharp
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
```

### 4.2 ProductCrud.Application

#### Common/Exceptions/NotFoundException.cs

```csharp
namespace ProductCrud.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested record does not exist.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id {key} was not found.") { }
}
```

#### Common/Interfaces/IProductRepository.cs

```csharp
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
```

#### Products/DTOs/ProductDto.cs

```csharp
namespace ProductCrud.Application.Products.DTOs;

/// <summary>
/// Data returned to the outside world (GraphQL). Never expose the entity directly.
/// </summary>
public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
```

#### Products/DTOs/ProductRequests.cs

```csharp
namespace ProductCrud.Application.Products.DTOs;

/// <summary>Data needed to create a product.</summary>
public record CreateProductRequest(string Name, string? Description, decimal Price, int Quantity);

/// <summary>Data needed to update a product.</summary>
public record UpdateProductRequest(int Id, string Name, string? Description, decimal Price, int Quantity);
```

#### Products/Mappings/ProductMappings.cs

```csharp
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
```

#### Products/Interfaces/IProductService.cs

```csharp
using ProductCrud.Application.Products.DTOs;

namespace ProductCrud.Application.Products.Interfaces;

/// <summary>
/// Product use cases exposed to the presentation layer (API).
/// </summary>
public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

#### Products/Services/ProductService.cs

```csharp
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
```

#### DependencyInjection.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using ProductCrud.Application.Products.Interfaces;
using ProductCrud.Application.Products.Services;

namespace ProductCrud.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}
```

### 4.3 ProductCrud.Infrastructure

#### Persistence/AppDbContext.cs

```csharp
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
```

#### Persistence/Configurations/ProductConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCrud.Domain.Entities;

namespace ProductCrud.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(Product.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(Product.DescriptionMaxLength);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.Property(p => p.Quantity)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}
```

#### Persistence/Repositories/ProductRepository.cs

```csharp
using Microsoft.EntityFrameworkCore;
using ProductCrud.Application.Common.Interfaces;
using ProductCrud.Domain.Entities;

namespace ProductCrud.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

#### DependencyInjection.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCrud.Application.Common.Interfaces;
using ProductCrud.Infrastructure.Persistence;
using ProductCrud.Infrastructure.Persistence.Repositories;

namespace ProductCrud.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        // Migrations live in this project (same assembly as AppDbContext)
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
```

### 4.4 ProductCrud.API

#### GraphQL/Inputs/ProductInputs.cs

```csharp
namespace ProductCrud.API.GraphQL.Inputs;

// GraphQL input types. Names match the frontend mutations:
//   mutation CreateProduct($input: CreateProductInput!)
//   mutation UpdateProduct($input: UpdateProductInput!)

public record CreateProductInput(string Name, string? Description, decimal Price, int Quantity);

public record UpdateProductInput(int Id, string Name, string? Description, decimal Price, int Quantity);
```

#### GraphQL/Queries/ProductQueries.cs

```csharp
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
```

#### GraphQL/Mutations/ProductMutations.cs

```csharp
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
```

#### GraphQL/Errors/GraphQLErrorFilter.cs

Sends readable validation / not-found messages to the client instead of "Unexpected Execution Error".

```csharp
using HotChocolate;
using ProductCrud.Application.Common.Exceptions;
using ProductCrud.Domain.Exceptions;

namespace ProductCrud.API.GraphQL.Errors;

/// <summary>
/// Converts known exceptions into readable GraphQL errors for the frontend.
/// </summary>
public class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error) => error.Exception switch
    {
        DomainException ex => error
            .WithMessage(ex.Message)
            .WithCode("VALIDATION_ERROR")
            .RemoveException(),

        NotFoundException ex => error
            .WithMessage(ex.Message)
            .WithCode("NOT_FOUND")
            .RemoveException(),

        _ => error
    };
}
```

#### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProductCrudDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> Using LocalDB? Use `Server=(localdb)\\MSSQLLocalDB;...` instead.

#### appsettings.Development.json

`IncludeScopes: true` shows the correlation ID on each console log line.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    },
    "Console": {
      "IncludeScopes": true
    }
  }
}
```

#### Properties/launchSettings.json

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "graphql",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

#### Program.cs

```csharp
using ProductCrud.API.GraphQL.Errors;
using ProductCrud.API.GraphQL.Mutations;
using ProductCrud.API.GraphQL.Queries;
using ProductCrud.API.Middleware;
using ProductCrud.Application;
using ProductCrud.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for the Next.js frontend
const string FrontendCors = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy => policy
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders(CorrelationIdMiddleware.HeaderName)); // let the browser read the ID
});

// GraphQL (Hot Chocolate)
builder.Services
    .AddGraphQLServer()
    .AddQueryType<ProductQueries>()
    .AddMutationType<ProductMutations>()
    .AddErrorFilter<GraphQLErrorFilter>()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

// Middleware pipeline (order matters)
app.UseApiMiddleware();      // CorrelationId -> RequestLogging -> ExceptionHandling
app.UseCors(FrontendCors);

app.MapGraphQL(); // http://localhost:5000/graphql (Nitro IDE in the browser)

app.Run();
```

---

## 5. Middleware

### Request pipeline

```text
HTTP request
  -> CorrelationIdMiddleware       sets X-Correlation-ID (reuses the caller's or creates a GUID)
  -> RequestLoggingMiddleware      logs "HTTP POST /graphql responded 200 in 12 ms"
  -> ExceptionHandlingMiddleware   unhandled exception -> JSON error
  -> CORS
  -> /graphql (Hot Chocolate -> resolvers -> GraphQLErrorFilter)
```

| Middleware | Purpose |
|---|---|
| `CorrelationIdMiddleware` | One ID per request, returned in the `X-Correlation-ID` header and added to every log line |
| `RequestLoggingMiddleware` | Logs method, path, status and duration. 2xx/3xx = Information, 4xx = Warning, 5xx = Error |
| `ExceptionHandlingMiddleware` | Global safety net for errors **outside** GraphQL resolvers |

> Errors thrown **inside** resolvers (validation, not found) are handled by `GraphQLErrorFilter`,
> because Hot Chocolate catches them before they reach the middleware.

### Middleware/ErrorResponse.cs

```csharp
using System.Text.Json.Serialization;

namespace ProductCrud.API.Middleware;

/// <summary>
/// JSON body returned by ExceptionHandlingMiddleware.
/// Example: { "code": "INTERNAL_ERROR", "message": "An unexpected error occurred.", "traceId": "..." }
/// </summary>
public record ErrorResponse(
    string Code,
    string Message,
    string TraceId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Detail = null);
```

### Middleware/CorrelationIdMiddleware.cs

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ProductCrud.API.Middleware;

/// <summary>
/// Gives every request an ID so all log lines from one request can be linked.
/// - Reuses the incoming X-Correlation-ID header if the caller sent one.
/// - Otherwise generates a new GUID.
/// - Returns the ID in the response header and adds it to the logging scope.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const int MaxLength = 64;

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        // Used by ExceptionHandlingMiddleware as the traceId in error responses
        context.TraceIdentifier = correlationId;

        // Set before the response starts, so the header is always sent
        context.Response.Headers[HeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].ToString();

        // Ignore missing or suspiciously long values (protects the logs)
        return string.IsNullOrWhiteSpace(incoming) || incoming.Length > MaxLength
            ? Guid.NewGuid().ToString()
            : incoming;
    }
}
```

### Middleware/RequestLoggingMiddleware.cs

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ProductCrud.API.Middleware;

/// <summary>
/// Logs one line per request: method, path, status code and duration.
/// Log level: 2xx/3xx = Information, 4xx = Warning, 5xx or exception = Error.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var failed = false;

        try
        {
            await _next(context);
        }
        catch
        {
            failed = true;
            throw; // let the exception continue; we only log it
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;

            var level = failed || statusCode >= 500 ? LogLevel.Error
                : statusCode >= 400 ? LogLevel.Warning
                : LogLevel.Information;

            _logger.Log(level,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                context.Request.Method,
                context.Request.Path.Value,
                failed ? 500 : statusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### Middleware/ExceptionHandlingMiddleware.cs

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductCrud.Application.Common.Exceptions;
using ProductCrud.Domain.Exceptions;

namespace ProductCrud.API.Middleware;

/// <summary>
/// Global safety net. Catches any unhandled exception in the HTTP pipeline and
/// returns a consistent JSON error instead of an empty 500 / HTML error page.
///
/// Note: exceptions thrown inside GraphQL resolvers are handled by Hot Chocolate
/// and GraphQLErrorFilter. This middleware covers everything outside of that.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected; nobody is waiting for a response.
            _logger.LogInformation("Request was cancelled by the client.");
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                // Too late to change the status code or body.
                _logger.LogError(ex, "Unhandled exception after the response started.");
                throw;
            }

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        var (statusCode, code, message) = ex switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "VALIDATION_ERROR", ex.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND", ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
        };

        if (statusCode >= 500)
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        else
            _logger.LogWarning("Handled {Code}: {Message}", code, ex.Message);

        // Show technical details only while developing
        var detail = statusCode >= 500 && _environment.IsDevelopment() ? ex.ToString() : null;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(
            new ErrorResponse(code, message, context.TraceIdentifier, detail));
    }
}
```

### Middleware/MiddlewareExtensions.cs

```csharp
using Microsoft.AspNetCore.Builder;

namespace ProductCrud.API.Middleware;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Registers the custom middleware in the correct order.
    /// 1. CorrelationId   - must be first so every log line has the ID
    /// 2. RequestLogging  - wraps everything below, so it logs the final status code
    /// 3. ExceptionHandling - converts unhandled exceptions into a JSON response
    /// </summary>
    public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
```

### Example error body

```json
{ "code": "INTERNAL_ERROR", "message": "An unexpected error occurred.", "traceId": "3f2c..." }
```

`detail` (the stack trace) is added only in **Development**.

---

## 6. Database Migration

The `AppDbContext` lives in **ProductCrud.Infrastructure**, while the startup project is **ProductCrud.API**, so both must be specified. Migrations are saved in `Persistence/Migrations`.

### Option A – Visual Studio Package Manager Console

Set **Default project** to `ProductCrud.Infrastructure`:

```powershell
Add-Migration InitialCreate -Project ProductCrud.Infrastructure -StartupProject ProductCrud.API -OutputDir Persistence/Migrations
Update-Database -Project ProductCrud.Infrastructure -StartupProject ProductCrud.API
```

### Option B – .NET CLI (from `backend\`)

```bash
dotnet ef migrations add InitialCreate -p src/ProductCrud.Infrastructure -s src/ProductCrud.API -o Persistence/Migrations
dotnet ef database update -p src/ProductCrud.Infrastructure -s src/ProductCrud.API
```

### Moving from the old layered version

The old `ProductCrudDb` already has the old `InitialCreate` applied. Choose one:

```powershell
# Option 1 – drop and recreate (deletes existing data)
Drop-Database -Project ProductCrud.Infrastructure -StartupProject ProductCrud.API
```

Option 2 – keep the old data: change the database name in `DefaultConnection`, e.g. `Database=ProductCrudCleanDb`.

### Verify

In SSMS: **ProductCrudDb → Tables** should contain:

- `dbo.Products`
- `dbo.__EFMigrationsHistory`

### Future schema changes

Always use a **new** migration name:

```powershell
Add-Migration AddCategory -Project ProductCrud.Infrastructure -StartupProject ProductCrud.API -OutputDir Persistence/Migrations
Update-Database -Project ProductCrud.Infrastructure -StartupProject ProductCrud.API
```

---

## 7. Run and Test the Backend

```powershell
cd C:\Project\ProductCrud\backend
dotnet run --project src/ProductCrud.API --launch-profile http
```

Open **http://localhost:5000/graphql** (Nitro UI) → **Create Document** → paste an operation → **Run** (Ctrl + Alt + Enter).

### Create

```graphql
mutation {
  createProduct(input: { name: "Mouse", description: "Wireless", price: 499.50, quantity: 20 }) {
    id name price quantity
  }
}
```

### Read all

```graphql
query {
  products { id name description price quantity createdAt updatedAt }
}
```

### Read by ID

```graphql
query {
  product(id: 1) { id name price quantity }
}
```

### Update

```graphql
mutation {
  updateProduct(input: { id: 1, name: "Mouse Pro", description: "Wireless", price: 650, quantity: 15 }) {
    id name price quantity updatedAt
  }
}
```

### Delete

```graphql
mutation {
  deleteProduct(id: 1)
}
```

### Error responses

| Exception | GraphQL code | Example message |
|---|---|---|
| `DomainException` | `VALIDATION_ERROR` | Product name is required. |
| `NotFoundException` | `NOT_FOUND` | Product with id 99 was not found. |

Try `createProduct(input: { name: "", price: 1, quantity: 1 })` to see a `VALIDATION_ERROR`.

### Check the middleware

The console should show one line per request with a correlation ID, e.g.:

```text
info: ProductCrud.API.Middleware.RequestLoggingMiddleware[0]
      => CorrelationId:3f2c...
      HTTP POST /graphql responded 200 in 15 ms
```

The response headers (browser DevTools → Network) include `X-Correlation-ID`.

> Tips: keep one operation per tab (or name them), use Ctrl + Space for autocomplete, and use **Browse Schema** to view all types.

---

## 8. Unit Tests and TDD

### Test projects

| Project | What is tested | Tools |
|---|---|---|
| `ProductCrud.Domain.UnitTests` | Create/Update rules, trimming, boundaries, a failed update keeps old values | xUnit |
| `ProductCrud.Application.UnitTests` | All use cases; invalid or missing products are never saved or deleted | xUnit + Moq |
| `ProductCrud.Infrastructure.UnitTests` | Add, get (ordered), update, delete | xUnit + EF Core InMemory |
| `ProductCrud.API.UnitTests` | GraphQL error mapping, correlation ID, request logging, global exception handling | xUnit + Hot Chocolate + `DefaultHttpContext` |

Tests don't need SQL Server. Every test uses **Arrange / Act / Assert** and is named `MethodName_Scenario_ExpectedResult`.

### Run the tests

```bash
dotnet test                                                      # all tests
dotnet test tests/ProductCrud.Domain.UnitTests                   # one project
dotnet test --filter "FullyQualifiedName~ProductServiceTests"    # one class
dotnet test --collect:"XPlat Code Coverage"                      # with coverage
```

Visual Studio: **Test → Test Explorer → Run All Tests** (Ctrl+R, A).

### TDD workflow (Red → Green → Refactor)

| Step | What you do | Result |
|---|---|---|
| 🔴 **Red** | Write a small test for behaviour that doesn't exist yet | Test fails (or doesn't compile) |
| 🟢 **Green** | Write the **minimum** code to make that test pass | All tests pass |
| 🔵 **Refactor** | Clean up names/duplication without changing behaviour | All tests still pass |

Work from the **inside out**: Domain → Application → Infrastructure → API. See **`backend/docs/TDD-GUIDE.md`** for a full worked example and a practice exercise.

### 8.1 tests/ProductCrud.Domain.UnitTests/Entities/ProductTests.cs

```csharp
using ProductCrud.Domain.Entities;
using ProductCrud.Domain.Exceptions;

namespace ProductCrud.Domain.UnitTests.Entities;

/// <summary>
/// Tests the business rules of the Product entity.
/// Naming: MethodName_Scenario_ExpectedResult. Structure: Arrange / Act / Assert.
/// </summary>
public class ProductTests
{
    // ---------- Create ----------

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var product = Product.Create("Keyboard", "Mechanical", 2500m, 10);

        // Assert
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal("Mechanical", product.Description);
        Assert.Equal(2500m, product.Price);
        Assert.Equal(10, product.Quantity);
        Assert.True(product.CreatedAt >= before);
        Assert.Null(product.UpdatedAt);
    }

    [Fact]
    public void Create_TrimsNameAndDescription()
    {
        var product = Product.Create("  Mouse  ", "  Wireless  ", 499.50m, 5);

        Assert.Equal("Mouse", product.Name);
        Assert.Equal("Wireless", product.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyDescription_SetsDescriptionToNull(string? description)
    {
        var product = Product.Create("Mouse", description, 100m, 1);

        Assert.Null(product.Description);
    }

    [Fact]
    public void Create_WithZeroPriceAndQuantity_IsAllowed()
    {
        var product = Product.Create("Free Sample", null, 0m, 0);

        Assert.Equal(0m, product.Price);
        Assert.Equal(0, product.Quantity);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsDomainException(string name)
    {
        var ex = Assert.Throws<DomainException>(() => Product.Create(name, null, 100m, 1));

        Assert.Equal("Product name is required.", ex.Message);
    }

    [Fact]
    public void Create_WithNameLongerThanMax_ThrowsDomainException()
    {
        var longName = new string('A', Product.NameMaxLength + 1);

        Assert.Throws<DomainException>(() => Product.Create(longName, null, 100m, 1));
    }

    [Fact]
    public void Create_WithNameExactlyMaxLength_IsAllowed()
    {
        var name = new string('A', Product.NameMaxLength);

        var product = Product.Create(name, null, 100m, 1);

        Assert.Equal(Product.NameMaxLength, product.Name.Length);
    }

    [Fact]
    public void Create_WithDescriptionLongerThanMax_ThrowsDomainException()
    {
        var longDescription = new string('D', Product.DescriptionMaxLength + 1);

        Assert.Throws<DomainException>(() => Product.Create("Mouse", longDescription, 100m, 1));
    }

    [Fact]
    public void Create_WithNegativePrice_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => Product.Create("Mouse", null, -1m, 1));

        Assert.Equal("Price must not be negative.", ex.Message);
    }

    [Fact]
    public void Create_WithNegativeQuantity_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => Product.Create("Mouse", null, 1m, -1));

        Assert.Equal("Quantity must not be negative.", ex.Message);
    }

    // ---------- Update ----------

    [Fact]
    public void Update_WithValidData_ChangesPropertiesAndSetsUpdatedAt()
    {
        var product = Product.Create("Keyboard", null, 2500m, 10);

        product.Update("Keyboard Pro", "RGB", 3000m, 5);

        Assert.Equal("Keyboard Pro", product.Name);
        Assert.Equal("RGB", product.Description);
        Assert.Equal(3000m, product.Price);
        Assert.Equal(5, product.Quantity);
        Assert.NotNull(product.UpdatedAt);
    }

    [Fact]
    public void Update_DoesNotChangeCreatedAt()
    {
        var product = Product.Create("Keyboard", null, 2500m, 10);
        var createdAt = product.CreatedAt;

        product.Update("Keyboard Pro", null, 3000m, 5);

        Assert.Equal(createdAt, product.CreatedAt);
    }

    [Fact]
    public void Update_WithInvalidData_ThrowsAndKeepsOriginalValues()
    {
        var product = Product.Create("Keyboard", "Mechanical", 2500m, 10);

        Assert.Throws<DomainException>(() => product.Update("Keyboard Pro", null, -5m, 5));

        Assert.Equal("Keyboard", product.Name);
        Assert.Equal("Mechanical", product.Description);
        Assert.Equal(2500m, product.Price);
        Assert.Equal(10, product.Quantity);
        Assert.Null(product.UpdatedAt);
    }
}
```

### 8.2 tests/ProductCrud.Application.UnitTests/Products/ProductServiceTests.cs

```csharp
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
```

### 8.3 tests/ProductCrud.Infrastructure.UnitTests/Repositories/ProductRepositoryTests.cs

```csharp
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
```

### 8.4 tests/ProductCrud.API.UnitTests/GraphQL/GraphQLErrorFilterTests.cs

```csharp
using HotChocolate;
using ProductCrud.API.GraphQL.Errors;
using ProductCrud.Application.Common.Exceptions;
using ProductCrud.Domain.Exceptions;

namespace ProductCrud.API.UnitTests.GraphQL;

/// <summary>
/// Tests that exceptions are turned into readable GraphQL errors for the frontend.
/// </summary>
public class GraphQLErrorFilterTests
{
    private readonly GraphQLErrorFilter _sut = new();

    private static IError ErrorFrom(Exception ex) =>
        ErrorBuilder.New()
            .SetMessage("Unexpected Execution Error")
            .SetException(ex)
            .Build();

    [Fact]
    public void OnError_DomainException_ReturnsValidationError()
    {
        var result = _sut.OnError(ErrorFrom(new DomainException("Product name is required.")));

        Assert.Equal("Product name is required.", result.Message);
        Assert.Equal("VALIDATION_ERROR", result.Code);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void OnError_NotFoundException_ReturnsNotFound()
    {
        var result = _sut.OnError(ErrorFrom(new NotFoundException("Product", 99)));

        Assert.Equal("Product with id 99 was not found.", result.Message);
        Assert.Equal("NOT_FOUND", result.Code);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void OnError_UnknownException_IsNotChanged()
    {
        var original = ErrorFrom(new InvalidOperationException("boom"));

        var result = _sut.OnError(original);

        Assert.Same(original, result);
    }
}
```

### 8.5 tests/ProductCrud.API.UnitTests/Middleware/TestHelpers.cs

```csharp
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProductCrud.API.UnitTests.Middleware;

/// <summary>Logger that stores every log entry so tests can check them.</summary>
public class TestLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = new();
    public List<IReadOnlyDictionary<string, object>> Scopes { get; } = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        if (state is IReadOnlyDictionary<string, object> dict) Scopes.Add(dict);
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
        => Entries.Add((logLevel, formatter(state, exception)));

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

/// <summary>Simple IHostEnvironment so tests can switch Development / Production.</summary>
public class FakeHostEnvironment : IHostEnvironment
{
    public FakeHostEnvironment(string environmentName) => EnvironmentName = environmentName;

    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; } = "ProductCrud.API";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
```

### 8.6 tests/ProductCrud.API.UnitTests/Middleware/CorrelationIdMiddlewareTests.cs

```csharp
using Microsoft.AspNetCore.Http;
using ProductCrud.API.Middleware;

namespace ProductCrud.API.UnitTests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly TestLogger<CorrelationIdMiddleware> _logger = new();

    [Fact]
    public async Task InvokeAsync_WithoutHeader_GeneratesGuidAndSetsResponseHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var id = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        Assert.True(Guid.TryParse(id, out _));
        Assert.Equal(id, context.TraceIdentifier);
    }

    [Fact]
    public async Task InvokeAsync_WithHeader_ReusesIncomingId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "abc-123";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger);

        await middleware.InvokeAsync(context);

        Assert.Equal("abc-123", context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
        Assert.Equal("abc-123", context.TraceIdentifier);
    }

    [Fact]
    public async Task InvokeAsync_WithTooLongHeader_GeneratesNewId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] =
            new string('x', CorrelationIdMiddleware.MaxLength + 1);
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, _logger);

        await middleware.InvokeAsync(context);

        Assert.True(Guid.TryParse(context.TraceIdentifier, out _));
    }

    [Fact]
    public async Task InvokeAsync_CallsNextAndAddsLoggingScope()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, _logger);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        var scope = Assert.Single(_logger.Scopes);
        Assert.Equal(context.TraceIdentifier, scope["CorrelationId"]);
    }
}
```

### 8.7 tests/ProductCrud.API.UnitTests/Middleware/RequestLoggingMiddlewareTests.cs

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProductCrud.API.Middleware;

namespace ProductCrud.API.UnitTests.Middleware;

public class RequestLoggingMiddlewareTests
{
    private readonly TestLogger<RequestLoggingMiddleware> _logger = new();

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/graphql";
        return context;
    }

    [Theory]
    [InlineData(200, LogLevel.Information)]
    [InlineData(404, LogLevel.Warning)]
    [InlineData(500, LogLevel.Error)]
    public async Task InvokeAsync_LogsWithLevelBasedOnStatusCode(int statusCode, LogLevel expectedLevel)
    {
        // Arrange
        var context = CreateContext();
        var middleware = new RequestLoggingMiddleware(ctx =>
        {
            ctx.Response.StatusCode = statusCode;
            return Task.CompletedTask;
        }, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(expectedLevel, entry.Level);
        Assert.Contains("POST /graphql", entry.Message);
        Assert.Contains($"responded {statusCode}", entry.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrows_LogsErrorAndRethrows()
    {
        var context = CreateContext();
        var middleware = new RequestLoggingMiddleware(_ => throw new InvalidOperationException("boom"), _logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("responded 500", entry.Message);
    }
}
```

### 8.8 tests/ProductCrud.API.UnitTests/Middleware/ExceptionHandlingMiddlewareTests.cs

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using ProductCrud.API.Middleware;
using ProductCrud.Application.Common.Exceptions;
using ProductCrud.Domain.Exceptions;

namespace ProductCrud.API.UnitTests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly TestLogger<ExceptionHandlingMiddleware> _logger = new();

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext { TraceIdentifier = "trace-1" };
        context.Response.Body = new MemoryStream();
        return context;
    }

    private ExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next, string environment = "Production")
        => new(next, _logger, new FakeHostEnvironment(environment));

    private static async Task<JsonElement> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);
        return doc.RootElement.Clone();
    }

    [Fact]
    public async Task InvokeAsync_WithoutException_PassesThrough()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task InvokeAsync_DomainException_Returns400ValidationError()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(_ => throw new DomainException("Product name is required."));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("code").GetString());
        Assert.Equal("Product name is required.", body.GetProperty("message").GetString());
        Assert.Equal("trace-1", body.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(_ => throw new NotFoundException("Product", 99));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Equal("NOT_FOUND", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task InvokeAsync_UnknownException_InProduction_HidesDetails()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("secret connection info"));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        var body = await ReadBodyAsync(context);
        Assert.Equal("INTERNAL_ERROR", body.GetProperty("code").GetString());
        Assert.Equal("An unexpected error occurred.", body.GetProperty("message").GetString());
        Assert.False(body.TryGetProperty("detail", out _));
    }

    [Fact]
    public async Task InvokeAsync_UnknownException_InDevelopment_IncludesDetails()
    {
        var context = CreateContext();
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("boom"), Environments.Development);

        await middleware.InvokeAsync(context);

        var body = await ReadBodyAsync(context);
        Assert.Contains("boom", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task InvokeAsync_ClientCancelled_DoesNotWriteError()
    {
        var context = CreateContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        context.RequestAborted = cts.Token;
        var middleware = CreateMiddleware(_ => throw new OperationCanceledException());

        await middleware.InvokeAsync(context);

        Assert.Equal(0, context.Response.Body.Length);
    }
}
```

---

## 9. Frontend Setup

> The frontend did **not** change. It uses the same `/graphql` endpoint, the same
> `CreateProductInput` / `UpdateProductInput` types, and the same operation names.

### 9.1 Create the Next.js app (folder must be empty)

```powershell
cd C:\Project\ProductCrud\frontend
npx create-next-app@latest . --typescript --tailwind --eslint --app --src-dir --import-alias "@/*" --use-npm
```

Accept defaults for any other prompts.

### 9.2 Install packages

```powershell
npm install @apollo/client@3 graphql
```

Apollo Client is pinned to v3 because v4 changes the hook import paths.

### 9.3 Clean up default files

- Delete `src/app/page.module.css` (if it exists).
- Replace all content of `src/app/globals.css` with:

```css
@import "tailwindcss";
```

### 9.4 Create the folders (PowerShell, from `frontend\`)

```powershell
New-Item -ItemType Directory -Force -Path `
  "src\app\(main)\products", `
  "src\components\ui", `
  "src\config", `
  "src\features\products\api", `
  "src\features\products\components", `
  "src\features\products\hooks", `
  "src\features\products\types", `
  "src\features\products\utils", `
  "src\lib\apollo", `
  "src\lib\utils", `
  "src\providers"
```

---

## 10. Frontend Code

### 10.1 Environment and config

#### .env.local (frontend root, next to package.json)

```env
NEXT_PUBLIC_GRAPHQL_URL=http://localhost:5000/graphql
```

#### src/config/env.ts

```ts
export const env = {
  graphqlUrl: process.env.NEXT_PUBLIC_GRAPHQL_URL ?? "http://localhost:5000/graphql",
};
```

### 10.2 lib and providers

#### src/lib/apollo/apollo-client.ts

```ts
import { ApolloClient, HttpLink, InMemoryCache } from "@apollo/client";
import { env } from "@/config/env";

export const apolloClient = new ApolloClient({
  link: new HttpLink({ uri: env.graphqlUrl }),
  cache: new InMemoryCache(),
});
```

#### src/lib/utils/format.ts

```ts
export const formatCurrency = (value: number): string =>
  Number(value).toLocaleString("en-PH", { style: "currency", currency: "PHP" });

export const formatDate = (value: string | null): string =>
  value ? new Date(value).toLocaleString("en-PH") : "-";
```

#### src/lib/utils/error.ts

```ts
export const getErrorMessage = (error: unknown): string =>
  error instanceof Error ? error.message : "An unexpected error occurred.";
```

#### src/providers/AppProviders.tsx

```tsx
"use client";
import { ApolloProvider } from "@apollo/client";
import { apolloClient } from "@/lib/apollo/apollo-client";

export default function AppProviders({ children }: { children: React.ReactNode }) {
  return <ApolloProvider client={apolloClient}>{children}</ApolloProvider>;
}
```

### 10.3 Shared UI components

#### src/components/ui/Button.tsx

```tsx
import { ButtonHTMLAttributes } from "react";

type Variant = "primary" | "secondary" | "warning" | "danger";

const variants: Record<Variant, string> = {
  primary: "bg-blue-600 text-white hover:bg-blue-700",
  secondary: "bg-gray-200 text-gray-800 hover:bg-gray-300",
  warning: "bg-yellow-500 text-white hover:bg-yellow-600",
  danger: "bg-red-600 text-white hover:bg-red-700",
};

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
}

export default function Button({ variant = "primary", className = "", type = "button", ...props }: ButtonProps) {
  return (
    <button
      type={type}
      className={`rounded px-4 py-2 text-sm font-medium transition disabled:cursor-not-allowed disabled:opacity-50 ${variants[variant]} ${className}`}
      {...props}
    />
  );
}
```

#### src/components/ui/Input.tsx

```tsx
import { InputHTMLAttributes } from "react";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  isRequired?: boolean;
}

export default function Input({ label, id, isRequired, className = "", ...props }: InputProps) {
  return (
    <div>
      <label htmlFor={id} className="mb-1 block text-sm font-medium text-gray-700">
        {label}
        {isRequired && <span className="text-red-600"> *</span>}
      </label>
      <input
        id={id}
        className={`w-full rounded border border-gray-300 bg-white px-3 py-2 text-sm focus:border-blue-500 focus:outline-none ${className}`}
        {...props}
      />
    </div>
  );
}
```

#### src/components/ui/Alert.tsx

```tsx
export interface AlertMessage {
  type: "success" | "error";
  text: string;
}

const styles: Record<AlertMessage["type"], string> = {
  success: "bg-green-100 text-green-800",
  error: "bg-red-100 text-red-800",
};

export default function Alert({ message, onClose }: { message: AlertMessage; onClose?: () => void }) {
  return (
    <div className={`mb-4 flex items-center justify-between rounded p-3 text-sm ${styles[message.type]}`}>
      <span>{message.text}</span>
      {onClose && (
        <button type="button" onClick={onClose} className="ml-4 font-bold">
          ×
        </button>
      )}
    </div>
  );
}
```

### 10.4 Products feature module (create in this order)

#### src/features/products/types/product.types.ts

```ts
export interface Product {
  id: number;
  name: string;
  description: string | null;
  price: number;
  quantity: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface ProductFormValues {
  name: string;
  description: string;
  price: string;
  quantity: string;
}

export interface CreateProductInput {
  name: string;
  description: string | null;
  price: number;
  quantity: number;
}

export interface UpdateProductInput extends CreateProductInput {
  id: number;
}

export interface GetProductsResponse {
  products: Product[];
}
```

#### src/features/products/api/product.queries.ts

```ts
import { gql } from "@apollo/client";

export const GET_PRODUCTS = gql`
  query GetProducts {
    products {
      id
      name
      description
      price
      quantity
      createdAt
      updatedAt
    }
  }
`;
```

#### src/features/products/api/product.mutations.ts

```ts
import { gql } from "@apollo/client";

export const CREATE_PRODUCT = gql`
  mutation CreateProduct($input: CreateProductInput!) {
    createProduct(input: $input) {
      id
      name
    }
  }
`;

export const UPDATE_PRODUCT = gql`
  mutation UpdateProduct($input: UpdateProductInput!) {
    updateProduct(input: $input) {
      id
      name
    }
  }
`;

export const DELETE_PRODUCT = gql`
  mutation DeleteProduct($id: Int!) {
    deleteProduct(id: $id)
  }
`;
```

#### src/features/products/utils/product.validation.ts

```ts
import { CreateProductInput, ProductFormValues } from "../types/product.types";

export type ProductValidationResult =
  | { ok: true; input: CreateProductInput }
  | { ok: false; error: string };

export function validateProductForm(values: ProductFormValues): ProductValidationResult {
  const name = values.name.trim();
  const price = Number(values.price);
  const quantity = Number(values.quantity);

  if (!name) return { ok: false, error: "Product name is required." };
  if (name.length > 150) return { ok: false, error: "Product name must not exceed 150 characters." };
  if (values.price === "" || Number.isNaN(price) || price < 0)
    return { ok: false, error: "Enter a valid price." };
  if (values.quantity === "" || !Number.isInteger(quantity) || quantity < 0)
    return { ok: false, error: "Enter a valid whole-number quantity." };

  return {
    ok: true,
    input: {
      name,
      description: values.description.trim() || null,
      price,
      quantity,
    },
  };
}
```

#### src/features/products/hooks/useProducts.ts

```ts
"use client";
import { useMutation, useQuery } from "@apollo/client";
import { GET_PRODUCTS } from "../api/product.queries";
import { CREATE_PRODUCT, DELETE_PRODUCT, UPDATE_PRODUCT } from "../api/product.mutations";
import { CreateProductInput, GetProductsResponse, UpdateProductInput } from "../types/product.types";

const refetchOptions = {
  refetchQueries: [{ query: GET_PRODUCTS }],
  awaitRefetchQueries: true,
};

export function useProducts() {
  const { data, loading, error } = useQuery<GetProductsResponse>(GET_PRODUCTS);
  const [createMutation, { loading: creating }] = useMutation(CREATE_PRODUCT, refetchOptions);
  const [updateMutation, { loading: updating }] = useMutation(UPDATE_PRODUCT, refetchOptions);
  const [deleteMutation, { loading: deleting }] = useMutation(DELETE_PRODUCT, refetchOptions);

  return {
    products: data?.products ?? [],
    loading,
    error,
    saving: creating || updating,
    deleting,
    createProduct: (input: CreateProductInput) => createMutation({ variables: { input } }),
    updateProduct: (input: UpdateProductInput) => updateMutation({ variables: { input } }),
    deleteProduct: (id: number) => deleteMutation({ variables: { id } }),
  };
}
```

#### src/features/products/components/ProductForm.tsx

> `handleChange` copies the previous state and updates **only the field that changed** (`next[field] = value`). If it doesn't, the inputs won't accept typing.

```tsx
"use client";
import { FormEvent, useState } from "react";
import Button from "@/components/ui/Button";
import Input from "@/components/ui/Input";
import { Product, ProductFormValues } from "../types/product.types";

const emptyForm: ProductFormValues = { name: "", description: "", price: "", quantity: "" };

const toFormValues = (product: Product | null): ProductFormValues =>
  product
    ? {
        name: product.name,
        description: product.description ?? "",
        price: String(product.price),
        quantity: String(product.quantity),
      }
    : emptyForm;

interface ProductFormProps {
  product: Product | null;
  saving: boolean;
  onSubmit: (values: ProductFormValues) => void;
  onCancel: () => void;
}

export default function ProductForm({ product, saving, onSubmit, onCancel }: ProductFormProps) {
  const [form, setForm] = useState<ProductFormValues>(() => toFormValues(product));

  // Updates only the field that changed
  const handleChange = (field: keyof ProductFormValues, value: string) => {
    setForm((prev) => {
      const next = { ...prev };
      next[field] = value;
      return next;
    });
  };

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    onSubmit(form);
  };

  return (
    <form onSubmit={handleSubmit} className="mb-8 rounded-lg bg-white p-5 shadow">
      <h2 className="mb-4 text-lg font-semibold">
        {product ? `Edit Product #${product.id}` : "Add Product"}
      </h2>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <Input
          id="name"
          label="Name"
          isRequired
          maxLength={150}
          value={form.name}
          onChange={(e) => handleChange("name", e.target.value)}
        />
        <Input
          id="description"
          label="Description"
          maxLength={500}
          value={form.description}
          onChange={(e) => handleChange("description", e.target.value)}
        />
        <Input
          id="price"
          label="Price"
          isRequired
          type="number"
          step="0.01"
          min="0"
          value={form.price}
          onChange={(e) => handleChange("price", e.target.value)}
        />
        <Input
          id="quantity"
          label="Quantity"
          isRequired
          type="number"
          step="1"
          min="0"
          value={form.quantity}
          onChange={(e) => handleChange("quantity", e.target.value)}
        />
      </div>

      <div className="mt-4 flex gap-2">
        <Button type="submit" disabled={saving}>
          {saving ? "Saving..." : product ? "Update" : "Create"}
        </Button>
        {product && (
          <Button variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
        )}
      </div>
    </form>
  );
}
```

#### src/features/products/components/ProductTable.tsx

```tsx
"use client";
import Button from "@/components/ui/Button";
import { formatCurrency } from "@/lib/utils/format";
import { Product } from "../types/product.types";

interface ProductTableProps {
  products: Product[];
  deleting: boolean;
  onEdit: (product: Product) => void;
  onDelete: (product: Product) => void;
}

export default function ProductTable({ products, deleting, onEdit, onDelete }: ProductTableProps) {
  if (products.length === 0) {
    return <p className="p-5 text-gray-500">No products found.</p>;
  }

  return (
    <table className="w-full text-left text-sm">
      <thead className="bg-gray-50">
        <tr>
          <th className="px-4 py-3">ID</th>
          <th className="px-4 py-3">Name</th>
          <th className="px-4 py-3">Description</th>
          <th className="px-4 py-3 text-right">Price</th>
          <th className="px-4 py-3 text-right">Qty</th>
          <th className="px-4 py-3 text-center">Actions</th>
        </tr>
      </thead>
      <tbody>
        {products.map((p) => (
          <tr key={p.id} className="border-t">
            <td className="px-4 py-3">{p.id}</td>
            <td className="px-4 py-3 font-medium">{p.name}</td>
            <td className="px-4 py-3 text-gray-600">{p.description ?? "-"}</td>
            <td className="px-4 py-3 text-right">{formatCurrency(p.price)}</td>
            <td className="px-4 py-3 text-right">{p.quantity}</td>
            <td className="px-4 py-3">
              <div className="flex justify-center gap-2">
                <Button variant="warning" className="px-3 py-1" onClick={() => onEdit(p)}>
                  Edit
                </Button>
                <Button variant="danger" className="px-3 py-1" disabled={deleting} onClick={() => onDelete(p)}>
                  Delete
                </Button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
```

#### src/features/products/components/ProductsView.tsx

```tsx
"use client";
import { useState } from "react";
import Alert, { AlertMessage } from "@/components/ui/Alert";
import { getErrorMessage } from "@/lib/utils/error";
import { useProducts } from "../hooks/useProducts";
import { Product, ProductFormValues } from "../types/product.types";
import { validateProductForm } from "../utils/product.validation";
import ProductForm from "./ProductForm";
import ProductTable from "./ProductTable";

export default function ProductsView() {
  const { products, loading, error, saving, deleting, createProduct, updateProduct, deleteProduct } =
    useProducts();

  const [editing, setEditing] = useState<Product | null>(null);
  const [formKey, setFormKey] = useState(0); // changing the key resets the form
  const [message, setMessage] = useState<AlertMessage | null>(null);

  const resetForm = () => {
    setEditing(null);
    setFormKey((k) => k + 1);
  };

  const handleSubmit = async (values: ProductFormValues) => {
    setMessage(null);

    const result = validateProductForm(values);
    if (!result.ok) {
      setMessage({ type: "error", text: result.error });
      return;
    }

    try {
      if (editing) {
        await updateProduct({ id: editing.id, ...result.input });
        setMessage({ type: "success", text: "Product updated." });
      } else {
        await createProduct(result.input);
        setMessage({ type: "success", text: "Product created." });
      }
      resetForm();
    } catch (err) {
      setMessage({ type: "error", text: getErrorMessage(err) });
    }
  };

  const handleEdit = (product: Product) => {
    setEditing(product);
    setFormKey((k) => k + 1);
    setMessage(null);
  };

  const handleDelete = async (product: Product) => {
    if (!confirm(`Delete "${product.name}"?`)) return;

    setMessage(null);
    try {
      await deleteProduct(product.id);
      if (editing?.id === product.id) resetForm();
      setMessage({ type: "success", text: "Product deleted." });
    } catch (err) {
      setMessage({ type: "error", text: getErrorMessage(err) });
    }
  };

  return (
    <section>
      <h1 className="mb-6 text-2xl font-bold">Products</h1>

      {message && <Alert message={message} onClose={() => setMessage(null)} />}

      <ProductForm
        key={formKey}
        product={editing}
        saving={saving}
        onSubmit={handleSubmit}
        onCancel={resetForm}
      />

      <div className="overflow-x-auto rounded-lg bg-white shadow">
        {loading ? (
          <p className="p-5">Loading products...</p>
        ) : error ? (
          <p className="p-5 text-red-600">Error: {error.message}</p>
        ) : (
          <ProductTable
            products={products}
            deleting={deleting}
            onEdit={handleEdit}
            onDelete={handleDelete}
          />
        )}
      </div>
    </section>
  );
}
```

#### src/features/products/index.ts

```ts
export { default as ProductsView } from "./components/ProductsView";
export { useProducts } from "./hooks/useProducts";
export type * from "./types/product.types";
```

### 10.5 App routes

#### src/app/layout.tsx (replace)

```tsx
import type { Metadata } from "next";
import "./globals.css";
import AppProviders from "@/providers/AppProviders";

export const metadata: Metadata = {
  title: "Product CRUD",
  description: "Next.js + GraphQL + ASP.NET Core",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className="bg-gray-100 text-gray-900">
        <AppProviders>{children}</AppProviders>
      </body>
    </html>
  );
}
```

#### src/app/page.tsx (replace)

```tsx
import { redirect } from "next/navigation";

export default function Home() {
  redirect("/products");
}
```

#### src/app/(main)/layout.tsx

```tsx
import Link from "next/link";

export default function MainLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen">
      <header className="bg-white shadow">
        <nav className="mx-auto flex max-w-5xl items-center gap-6 px-6 py-4">
          <span className="text-lg font-bold text-blue-600">ProductCrud</span>
          <Link href="/products" className="text-sm font-medium hover:text-blue-600">
            Products
          </Link>
        </nav>
      </header>
      <main className="mx-auto max-w-5xl p-6">{children}</main>
    </div>
  );
}
```

#### src/app/(main)/products/page.tsx

```tsx
import { ProductsView } from "@/features/products";

export default function ProductsPage() {
  return <ProductsView />;
}
```

---

## 11. Run the Full Application

Use two terminals:

```powershell
# Terminal 1 – Backend
cd C:\Project\ProductCrud\backend
dotnet run --project src/ProductCrud.API --launch-profile http

# Terminal 2 – Frontend
cd C:\Project\ProductCrud\frontend
npm run dev
```

Open **http://localhost:3000** (redirects to `/products`).

### Test checklist

- [ ] `dotnet test` passes all backend unit tests
- [ ] Page shows the header, form, and product table
- [ ] Typing works in Name, Description, Price, and Quantity
- [ ] **Create** adds a product and shows "Product created."
- [ ] **Edit** fills the form; **Update** saves; **Cancel** clears
- [ ] **Delete** asks for confirmation and removes the row
- [ ] Submitting an empty name shows a red validation message
- [ ] Backend console shows `HTTP POST /graphql responded 200 ...` with a `CorrelationId`
- [ ] Response headers include `X-Correlation-ID`

---

## 12. Adding a New Module

### Backend (e.g., Category) – write the tests first (TDD)

1. **Domain** – `Entities/Category.cs` with `Create()` / `Update()` and its rules.
   🔴 Start with `tests/ProductCrud.Domain.UnitTests/Entities/CategoryTests.cs`.
2. **Application** – `Categories/DTOs`, `Categories/Interfaces/ICategoryService.cs`, `Categories/Mappings`, `Categories/Services/CategoryService.cs`, and `Common/Interfaces/ICategoryRepository.cs`. Register the service in `DependencyInjection.cs`.
   🔴 Start with `CategoryServiceTests.cs` (mock `ICategoryRepository`).
3. **Infrastructure** – `Persistence/Configurations/CategoryConfiguration.cs`, `Persistence/Repositories/CategoryRepository.cs`, add `DbSet<Category>` to `AppDbContext`, and register the repository.
   🔴 Add `CategoryRepositoryTests.cs` (InMemory).
4. **API** – `GraphQL/Inputs/CategoryInputs.cs`, `CategoryQueries.cs`, `CategoryMutations.cs`. Because `ProductQueries` / `ProductMutations` are registered with `AddQueryType<>()` / `AddMutationType<>()`, add the new classes as type extensions:
   - mark them with `[ExtendObjectType(OperationTypeNames.Query)]` / `[ExtendObjectType(OperationTypeNames.Mutation)]`
   - register with `.AddTypeExtension<CategoryQueries>()` and `.AddTypeExtension<CategoryMutations>()` in `Program.cs`
5. Run a new migration:

```powershell
Add-Migration AddCategory -Project ProductCrud.Infrastructure -StartupProject ProductCrud.API -OutputDir Persistence/Migrations
Update-Database -Project ProductCrud.Infrastructure -StartupProject ProductCrud.API
```

### Frontend

- Create `src/features/categories/` with `api/`, `components/`, `hooks/`, `types/`, `utils/`, `index.ts`
- Add `src/app/(main)/categories/page.tsx` that renders `<CategoriesView />`
- Add a "Categories" link in `src/app/(main)/layout.tsx`

---

## 13. Troubleshooting

| Problem | Fix |
|---|---|
| `Your target project 'ProductCrud.API' doesn't match your migrations assembly` | Add `-Project ProductCrud.Infrastructure -StartupProject ProductCrud.API` to migration commands |
| `The name 'InitialCreate' is used by an existing migration` | Migration already exists – just run `Update-Database` |
| Migration has already been applied on `Remove-Migration` | Nothing to fix – the database is already up to date |
| `There is already an object named 'Products'` | Old database from the layered version. Run `Drop-Database ...` or use a new database name (see section 6) |
| `Connection string 'DefaultConnection' was not found` | Check `DefaultConnection` in `src/ProductCrud.API/appsettings.json` |
| Cannot type in form inputs | `handleChange` must update `next[field]`, not a key named `value` (see `ProductForm.tsx`) |
| `Failed to fetch` | Backend must be running on port 5000; restart `npm run dev` after editing `.env.local` |
| CORS error | Allow `http://localhost:3000` and keep `app.UseCors(...)` before `app.MapGraphQL()` |
| "Unexpected Execution Error" in GraphQL | Ensure `GraphQLErrorFilter` is registered with `.AddErrorFilter<>()` and the exception is a `DomainException` or `NotFoundException` |
| `A second operation was started on this context instance` | Two root fields that use the database were requested in one query and ran in parallel. Send them as separate requests, or make resolvers run one at a time |
| `CorrelationId` not shown in console logs | Set `"Console": { "IncludeScopes": true }` in `appsettings.Development.json` |
| Middleware tests: `HttpContext` / `DefaultHttpContext` not found | Add `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to the API test project |
| Tests not showing in Test Explorer | Rebuild the solution; make sure `xunit.runner.visualstudio` and `Microsoft.NET.Test.Sdk` are installed |
| Gray background / Tailwind styles missing | `globals.css` should only contain `@import "tailwindcss";` |
| Duplicate `/products` route | Delete the old `src/app/products/` folder; keep only `src/app/(main)/products/` |
| `Module not found: @/...` | Check the file is under `src/` and the path/casing matches exactly |
| Using LocalDB | Use `Server=(localdb)\\MSSQLLocalDB;...` in the connection string |
