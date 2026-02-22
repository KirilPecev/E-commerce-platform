using CatalogService.Application.Products.Commands;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests.ApplicationTests
{
    public class DeleteProductVariantCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldDeleteVariant_WhenExists()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("Watch"),
                new Money(120m, "USD"),
                category!,
                "Smart watch"
            );

            var variant = product.AddProductVariant("WCH-001", 120m, "USD", 7, "OneSize", "Black");

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // ensure variant exists
            var existsBefore = await dbContext.ProductVariants.AnyAsync(v => v.Id == variant.Id, TestContext.Current.CancellationToken);
            existsBefore.Should().BeTrue();

            var handler = new DeleteProductVariantCommandHandler(dbContext);

            await handler.Handle(new DeleteProductVariantCommand(product.Id, variant.Id), TestContext.Current.CancellationToken);

            var existsAfter = await dbContext.ProductVariants.AnyAsync(v => v.Id == variant.Id, TestContext.Current.CancellationToken);
            existsAfter.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldNotThrow_WhenVariantDoesNotExist()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new DeleteProductVariantCommandHandler(dbContext);

            Func<Task> act = async () => await handler.Handle(new DeleteProductVariantCommand(Guid.NewGuid(), Guid.NewGuid()), TestContext.Current.CancellationToken);

            await act.Should().NotThrowAsync();
        }
    }
}
