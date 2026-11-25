# TDD Guide for ProductCrud

**Test-Driven Development (TDD)** means you write a failing test **before** writing the code that makes it pass.

## The cycle: Red → Green → Refactor

| Step | What you do | Result |
|---|---|---|
| 🔴 **Red** | Write a small test for behaviour that doesn't exist yet | Test fails (or doesn't compile) |
| 🟢 **Green** | Write the **minimum** code to make that test pass | All tests pass |
| 🔵 **Refactor** | Clean up names/duplication without changing behaviour | All tests still pass |

Repeat for each small rule. Never write production code without a failing test asking for it.

## Rules used in this project

- **Arrange / Act / Assert** in every test.
- Test names: `MethodName_Scenario_ExpectedResult`, e.g. `Create_WithNegativePrice_ThrowsDomainException`.
- One behaviour per test. Use `[Theory]` + `[InlineData]` for the same rule with several inputs.
- Test from the **inside out**: Domain → Application → Infrastructure → API.
- Mock only what the class depends on (Application mocks `IProductRepository` with Moq).

## Worked example: how `Product` was built with TDD

### Cycle 1 – creating a product

🔴 **Red** – write the test first (`Product.Create` doesn't exist yet, so it won't compile):

```csharp
[Fact]
public void Create_WithValidData_SetsAllProperties()
{
    var product = Product.Create("Keyboard", "Mechanical", 2500m, 10);

    Assert.Equal("Keyboard", product.Name);
    Assert.Equal(2500m, product.Price);
    Assert.Equal(10, product.Quantity);
}
```

🟢 **Green** – the minimum code:

```csharp
public static Product Create(string name, string? description, decimal price, int quantity)
    => new Product { Name = name, Description = description, Price = price, Quantity = quantity };
```

### Cycle 2 – name is required

🔴 **Red**

```csharp
[Theory]
[InlineData("")]
[InlineData("   ")]
public void Create_WithEmptyName_ThrowsDomainException(string name)
{
    var ex = Assert.Throws<DomainException>(() => Product.Create(name, null, 100m, 1));
    Assert.Equal("Product name is required.", ex.Message);
}
```

🟢 **Green**

```csharp
if (string.IsNullOrWhiteSpace(name))
    throw new DomainException("Product name is required.");
```

### Cycle 3 – price must not be negative

🔴 Write `Create_WithNegativePrice_ThrowsDomainException` → fails.
🟢 Add `if (price < 0) throw new DomainException("Price must not be negative.");` → passes.

### Cycle 4 – update

🔴 Write `Update_WithValidData_ChangesPropertiesAndSetsUpdatedAt` → `Update` doesn't exist.
🟢 Add an `Update(...)` method that copies the checks and sets `UpdatedAt`.

### 🔵 Refactor

`Create` and `Update` now repeat the same checks. Move them into one private
`SetDetails(...)` method. Run the tests again – they all stay green, which proves the refactor
didn't break anything. Then add one more test,
`Update_WithInvalidData_ThrowsAndKeepsOriginalValues`, to lock in that values are only assigned
**after** every check passes.

## Example: TDD in the Application layer

🔴 **Red** – the service must not save an invalid product:

```csharp
[Fact]
public async Task CreateAsync_WithInvalidRequest_ThrowsAndDoesNotSave()
{
    var request = new CreateProductRequest("", null, 100m, 1);

    await Assert.ThrowsAsync<DomainException>(() => _sut.CreateAsync(request));

    _repository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
}
```

🟢 **Green** – create the entity (which validates) **before** calling the repository:

```csharp
var product = Product.Create(request.Name, request.Description, request.Price, request.Quantity);
await _repository.AddAsync(product, cancellationToken);
```

## Practice: add a new rule using TDD

Try this on your own: *"Quantity must not exceed 10,000."*

1. 🔴 In `ProductTests.cs`, add `Create_WithQuantityAboveMax_ThrowsDomainException`. Run tests → **red**.
2. 🟢 Add `public const int QuantityMax = 10_000;` and the check in `SetDetails`. Run tests → **green**.
3. 🔵 Make sure the message is clear and the constant is reused. Run tests → still **green**.
4. Add a boundary test: exactly `10_000` is allowed.

## Running tests

```bash
dotnet test                                   # all tests
dotnet test tests/ProductCrud.Domain.UnitTests  # one project
dotnet test --filter "FullyQualifiedName~ProductServiceTests"   # one class
dotnet test --collect:"XPlat Code Coverage"   # with coverage
```

Visual Studio: **Test → Test Explorer → Run All Tests** (Ctrl+R, A).
