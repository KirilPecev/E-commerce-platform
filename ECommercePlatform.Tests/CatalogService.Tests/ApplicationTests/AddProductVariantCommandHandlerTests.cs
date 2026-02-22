using CatalogService.Application.Products.Commands;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;
using CatalogService.Application.Exceptions;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests.ApplicationTests
{
    public class AddProductVariantCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldAddVariant_WhenProductExists()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("Speaker"),
                new Money(80m, "USD"),
                category!,
                "Bluetooth speaker"
            );

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new AddProductVariantCommandHandler(dbContext);

            var command = new AddProductVariantCommand(product.Id, "SPK-001", 80m, "USD", 12, "OneSize", "Black");

            var result = await handler.Handle(command, TestContext.Current.CancellationToken);

            result.Should().NotBeEmpty();

            var variant = await dbContext.ProductVariants.FirstOrDefaultAsync(v => v.Id == result, TestContext.Current.CancellationToken);

            variant.Should().NotBeNull();
            variant!.Sku.Should().Be("SPK-001");
            variant.Price.Amount.Should().Be(80m);
            variant.Price.Currency.Should().Be("USD");
            variant.Size.Should().Be("OneSize");
            variant.Color.Should().Be("Black");
            variant.StockQuantity.Should().Be(12);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenProductDoesNotExist()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new AddProductVariantCommandHandler(dbContext);

            var command = new AddProductVariantCommand(Guid.NewGuid(), "SPK-001", 80m, "USD", 12, "OneSize", "Black");

            Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
