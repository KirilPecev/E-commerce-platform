using CatalogService.Domain.Aggregates;
using CatalogService.Domain.Events;
using CatalogService.Domain.Exceptions;
using CatalogService.Domain.ValueObjects;

using FluentAssertions;

namespace CatalogService.Tests.DomainUnitTests
{
    public class ProductTests
    {
        [Fact]
        public void Create_ShouldCreateValidProduct()
        {
            // Act
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            // Assert
            product.Name.Value.Should().Be("Laptop");
            product.Price.Amount.Should().Be(1500m);
            product.Price.Currency.Should().Be("USD");
            product.Category.Name.Should().Be("Electronics");
            product.Description.Should().Be("High-end gaming laptop");
            product.Status.Should().Be(ProductStatus.Active);
            product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Update_ShouldChangeProductData()
        {
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            product.UpdateDetails(new ProductName("New Laptop"), new Category(Guid.Parse("11111111-0000-0000-0000-00000000000B"), "Office Supplies", "Stationery, office equipment and supplies."), "New Desc");

            product.Name.Value.Should().Be("New Laptop");
            product.Category.Name.Should().Be("Office Supplies");
            product.Description.Should().Be("New Desc");
        }

        [Fact]
        public void Update_ShouldChangeProductPrice()
        {
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            product.ChangePrice(new Money(1200m, "EUR"));

            product.Name.Value.Should().Be("Laptop");
            product.Category.Name.Should().Be("Electronics");
            product.Price.Amount.Should().Be(1200m);
            product.Price.Currency.Should().Be("EUR");
        }

        [Fact]
        public void Create_ShouldThrow_WhenPriceIsNegative()
        {
            Action act = () => new Product(
                new ProductName("Laptop"),
                new Money(-1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Create_ShouldThrow_WhenNoCurrency()
        {
            Action act = () => new Product(
                new ProductName("Laptop"),
                new Money(1500m, ""),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Create_ShouldThrow_WhenCurrencyIsBelow3Symbols()
        {
            Action act = () => new Product(
                new ProductName("Laptop"),
                new Money(1500m, "AS"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Create_ShouldThrow_WhenProductNameNotProvided()
        {
            Action act = () => new Product(
                new ProductName(""),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Create_ShouldThrow_WhenProductNameIsMoreThan100Symbols()
        {
            Action act = () => new Product(
                new ProductName("11111111-0000-0000-0000-00000000000111111111-0000-0000-0000-00000000000111111111-0000-0000-0000-000000000001"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Update_ShouldDeactivateProduct()
        {
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            product.Deactivate();

            product.Status.Should().Be(ProductStatus.Inactive);
        }

        [Fact]
        public void Update_ShouldActivateProduct()
        {
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            product.Deactivate();

            product.Activate();

            product.Status.Should().Be(ProductStatus.Active);
        }

        [Fact]
        public void Create_ShouldAddProductVariant()
        {
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            product.AddProductVariant(product.Id.ToString(), 1500m, "USD", 10, "15 inch", "Black");

            product.Variants.Should().ContainSingle(v => v.Sku == product.Id.ToString() && v.Price.Amount == 1500m && v.Price.Currency == "USD" && v.StockQuantity == 10 && v.Size == "15 inch" && v.Color == "Black");
            product.DomainEvents.Should().ContainSingle(e =>
                e is ProductCreatedDomainEvent);
        }

        [Fact]
        public void Create_ShouldThrow_WhenProductIsInactive()
        {
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            product.Deactivate();

            Action act = () => product.AddProductVariant(product.Id.ToString(), 1500m, "USD", 10, "15 inch", "Black");

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Create_ShouldChangeProductVariantPrice()
        {
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            ProductVariant variant = product.AddProductVariant(product.Id.ToString(), 1500m, "USD", 10, "15 inch", "Black");

            variant.ChangePrice(new Money(1200m, "EUR"));

            variant.Price.Amount.Should().Be(1200m);
        }

        [Fact]
        public void Create_ShouldThrow_WhenQuantityIsNegative()
        {
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            Action act = () => product.AddProductVariant(product.Id.ToString(), 1500m, "USD", -10, "15 inch", "Black");

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Update_ShouldChangeProductVariantData()
        {
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            ProductVariant variant = product.AddProductVariant(product.Id.ToString(), 1500m, "USD", 10, "15 inch", "Black");

            variant.UpdateDetails($"{product.Id.ToString()}-test", "16 inch", "Blue", 5);

            variant.Sku.Should().Be($"{product.Id.ToString()}-test");
            variant.Size.Should().Be("16 inch");
            variant.Color.Should().Be("Blue");
            variant.StockQuantity.Should().Be(5);
            variant.DomainEvents.Should().ContainSingle(e =>
               e is ProductUpdatedDomainEvent);
        }

        [Fact]
        public void Update_ShouldThrow_WhenQuantityIsNegavite()
        {
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "Phones, computers, audio/video and related accessories."),
                "High-end gaming laptop"
            );

            ProductVariant variant = product.AddProductVariant(product.Id.ToString(), 1500m, "USD", 10, "15 inch", "Black");

            Action act = () => variant.UpdateDetails($"{product.Id.ToString()}-test", "16 inch", "Blue", -5);

            act.Should().Throw<CatalogDomainException>();
        }
    }
}
