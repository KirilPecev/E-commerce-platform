using CatalogService.Application.Exceptions;
using CatalogService.Application.Products.Commands;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests.ApplicationTests
{
    public class UpdateProductVariantCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldUpdateVariant_WhenExists()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("Phone"),
                new Money(500m, "USD"),
                category!,
                "Smartphone"
            );

            var variant = product.AddProductVariant("PHN-001", 500m, "USD", 20, "64GB", "Black");

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new UpdateProductVariantCommandHandler(dbContext);

            var command = new UpdateProductVariantCommand(product.Id, variant.Id, "PHN-001-NEW", 450m, "EUR", 15, "128GB", "Blue");

            await handler.Handle(command, TestContext.Current.CancellationToken);

            var updated = await dbContext.ProductVariants.Include(v => v.Product).FirstOrDefaultAsync(v => v.Id == variant.Id, TestContext.Current.CancellationToken);

            updated.Should().NotBeNull();
            updated!.Sku.Should().Be("PHN-001-NEW");
            updated.Price.Amount.Should().Be(450m);
            updated.Price.Currency.Should().Be("EUR");
            updated.Size.Should().Be("128GB");
            updated.Color.Should().Be("Blue");
            updated.StockQuantity.Should().Be(15);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenVariantDoesNotExist()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new UpdateProductVariantCommandHandler(dbContext);

            var command = new UpdateProductVariantCommand(Guid.NewGuid(), Guid.NewGuid(), "SKU", 10m, "USD", 1, null, null);

            Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
