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
