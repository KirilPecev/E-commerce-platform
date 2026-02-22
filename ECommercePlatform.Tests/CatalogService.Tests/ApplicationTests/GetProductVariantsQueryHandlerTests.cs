using CatalogService.Application.Products.Queries;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests.ApplicationTests
{
    public class GetProductVariantsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnVariants_WhenProductHasVariants()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("Shoes"),
                new Money(50m, "USD"),
                category!,
                "Comfortable shoes"
            );

            var v1 = product.AddProductVariant("SKU-SHOE-1", 50m, "USD", 10, "9", "Black");
            var v2 = product.AddProductVariant("SKU-SHOE-2", 55m, "USD", 5, "10", "White");

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new GetProductVariantsQueryHandler(dbContext);

            var result = await handler.Handle(new GetProductVariantsQuery(product.Id), TestContext.Current.CancellationToken);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            result.Should().ContainSingle(r => r.Id == v1.Id && r.Sku == "SKU-SHOE-1" && r.Amount == 50m && r.Currency == "USD" && r.Size == "9" && r.Color == "Black" && r.StockQuantity == 10);
            result.Should().ContainSingle(r => r.Id == v2.Id && r.Sku == "SKU-SHOE-2" && r.Amount == 55m && r.Currency == "USD" && r.Size == "10" && r.Color == "White" && r.StockQuantity == 5);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenProductHasNoVariants()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("Hat"),
                new Money(15m, "USD"),
                category!,
                "Stylish hat"
            );

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new GetProductVariantsQueryHandler(dbContext);

            var result = await handler.Handle(new GetProductVariantsQuery(product.Id), TestContext.Current.CancellationToken);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
