# ProductCrud Backend (Clean Architecture + Unit Tests + TDD)

ASP.NET Core **.NET 8** + **Hot Chocolate GraphQL** + **EF Core 8** + **SQL Server**, tested with **xUnit**, **Moq** and **EF Core InMemory**.

## Project structure

```text
backend/
├── ProductCrud.sln
├── src/
│   ├── ProductCrud.Domain/                 # Enterprise rules (no dependencies)
│   │   ├── Entities/Product.cs
│   │   └── Exceptions/DomainException.cs
│   ├── ProductCrud.Application/            # Use cases (depends on Domain)
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
│   ├── ProductCrud.Infrastructure/         # EF Core + SQL Server (depends on Application)
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/ProductConfiguration.cs
│   │   │   ├── Repositories/ProductRepository.cs
│   │   │   └── Migrations/                 # created by Add-Migration
│   │   └── DependencyInjection.cs
│   └── ProductCrud.API/                    # GraphQL presentation (composition root)
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
│       └── Program.cs
├── tests/
│   ├── ProductCrud.Domain.UnitTests/          # Entities/ProductTests.cs
│   ├── ProductCrud.Application.UnitTests/     # Products/ProductServiceTests.cs
│   ├── ProductCrud.Infrastructure.UnitTests/  # Repositories/ProductRepositoryTests.cs
│   └── ProductCrud.API.UnitTests/             # GraphQL/ + Middleware/ tests
└── docs/
    └── TDD-GUIDE.md                           # Red → Green → Refactor walkthrough
```

## Dependency rule

```text
API ──► Application ──► Domain
 │            ▲
 └──► Infrastructure
```

- **Domain** references nothing.
- **Application** references Domain only and defines `IProductRepository`.
- **Infrastructure** implements `IProductRepository` with EF Core.
- **API** wires everything together (`AddApplication()`, `AddInfrastructure()`).
- Each **test project** references only the layer it tests.

| Layer | Responsibility |
|---|---|
| Domain | `Product` entity and its rules (`Create`, `Update`, validation) |
| Application | Use cases in `ProductService`, DTOs, contracts |
| Infrastructure | `AppDbContext`, table configuration, repository |
| API | GraphQL queries/mutations, error filter, middleware, CORS |

## Setup

1. Open `ProductCrud.sln` in Visual Studio 2022.
2. Update `DefaultConnection` in `src/ProductCrud.API/appsettings.json` if needed.
3. Right-click **ProductCrud.API** → **Set as Startup Project**.
4. Build the solution (packages restore automatically).

## Database migration (Package Manager Console)

```powershell
Add-Migration InitialCreate -Project ProductCrud.Infrastructure -StartupProject ProductCrud.API -OutputDir Persistence/Migrations
Update-Database -Project ProductCrud.Infrastructure -StartupProject ProductCrud.API
```

Or with the .NET CLI (from the `backend` folder):

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate -p src/ProductCrud.Infrastructure -s src/ProductCrud.API -o Persistence/Migrations
dotnet ef database update -p src/ProductCrud.Infrastructure -s src/ProductCrud.API
```

> If `ProductCrudDb` already exists from the old project, either run
> `Drop-Database -Project ProductCrud.Infrastructure -StartupProject ProductCrud.API` first,
> or change the database name in the connection string.

## Run

```bash
dotnet run --project src/ProductCrud.API --launch-profile http
```

Open **http://localhost:5000/graphql** (Nitro IDE).

## Unit tests

| Project | What is tested | Tools |
|---|---|---|
| `ProductCrud.Domain.UnitTests` | Create/Update rules, trimming, boundaries, a failed update keeps old values | xUnit |
| `ProductCrud.Application.UnitTests` | All use cases; invalid or missing products are never saved or deleted | xUnit + Moq |
| `ProductCrud.Infrastructure.UnitTests` | Add, get (ordered), update, delete | xUnit + EF Core InMemory |
| `ProductCrud.API.UnitTests` | GraphQL error mapping, correlation ID, request logging, global exception handling | xUnit + Hot Chocolate + DefaultHttpContext |

Tests don't need SQL Server.

```bash
dotnet test                                                      # all tests
dotnet test tests/ProductCrud.Domain.UnitTests                   # one project
dotnet test --filter "FullyQualifiedName~ProductServiceTests"    # one class
dotnet test --collect:"XPlat Code Coverage"                      # with coverage
```

Visual Studio: **Test → Test Explorer → Run All Tests** (Ctrl+R, A).

See **docs/TDD-GUIDE.md** for how the code is built test-first.

## Middleware

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
| `RequestLoggingMiddleware` | Method, path, status and duration. 2xx = Information, 4xx = Warning, 5xx = Error |
| `ExceptionHandlingMiddleware` | Global safety net for errors **outside** GraphQL resolvers |

Example error body from `ExceptionHandlingMiddleware`:

```json
{ "code": "INTERNAL_ERROR", "message": "An unexpected error occurred.", "traceId": "3f2c..." }
```

`detail` (the stack trace) is added only in **Development**.

> Errors thrown **inside** resolvers (validation, not found) are still handled by
> `GraphQLErrorFilter`, because Hot Chocolate catches them before they reach the middleware.

## GraphQL operations

```graphql
query {
  products { id name description price quantity createdAt updatedAt }
}

query {
  product(id: 1) { id name price }
}

mutation {
  createProduct(input: { name: "Keyboard", description: "Mechanical", price: 2500, quantity: 10 }) {
    id name
  }
}

mutation {
  updateProduct(input: { id: 1, name: "Keyboard Pro", description: null, price: 3000, quantity: 5 }) {
    id name
  }
}

mutation {
  deleteProduct(id: 1)
}
```

## Error codes

| Exception | GraphQL code | Example |
|---|---|---|
| `DomainException` | `VALIDATION_ERROR` | Product name is required. |
| `NotFoundException` | `NOT_FOUND` | Product with id 99 was not found. |

The frontend (`http://localhost:3000`) works without changes: same endpoint, same
`CreateProductInput` / `UpdateProductInput` types, and the same operation names.
