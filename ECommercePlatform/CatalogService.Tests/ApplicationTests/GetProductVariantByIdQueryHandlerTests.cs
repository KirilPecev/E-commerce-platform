using CatalogService.Application.Products.Queries;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests.ApplicationTests
{
    public class GetProductVariantByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnVariant_WhenExists()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("T-Shirt"),
                new Money(20m, "USD"),
                category!,
                "Test product"
            );

            var variant = product.AddProductVariant("SKU123", 20m, "USD", 5, "M", "Red");

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new GetProductVariantByIdQueryHandler(dbContext);

            var result = await handler.Handle(new GetProductVariantByIdQuery(product.Id, variant.Id), TestContext.Current.CancellationToken);

            result.Should().NotBeNull();
            result!.Id.Should().Be(variant.Id);
            result.Sku.Should().Be("SKU123");
            result.Amount.Should().Be(20m);
            result.Currency.Should().Be("USD");
            result.Size.Should().Be("M");
            result.Color.Should().Be("Red");
            result.StockQuantity.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenVariantNotFound()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("T-Shirt"),
                new Money(20m, "USD"),
                category!,
                "Test product"
            );

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new GetProductVariantByIdQueryHandler(dbContext);

            var result = await handler.Handle(new GetProductVariantByIdQuery(product.Id, Guid.NewGuid()), TestContext.Current.CancellationToken);

            result.Should().BeNull();
        }
    }
}
