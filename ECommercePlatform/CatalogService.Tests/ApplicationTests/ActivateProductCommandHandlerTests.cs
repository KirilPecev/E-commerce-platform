using CatalogService.Application.Products.Commands;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;
using CatalogService.Application.Exceptions;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace CatalogService.Tests.ApplicationTests
{
    public class ActivateProductCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldActivateProduct_WhenProductExists()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("Camera"),
                new Money(300m, "USD"),
                category!,
                "Photo camera"
            );

            product.Deactivate();

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new ActivateProductCommandHandler(dbContext);

            await handler.Handle(new ActivateProductCommand(product.Id), TestContext.Current.CancellationToken);

            var updated = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id, TestContext.Current.CancellationToken);

            updated.Should().NotBeNull();
            updated!.Status.Should().Be(ProductStatus.Active);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenProductDoesNotExist()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new ActivateProductCommandHandler(dbContext);

            Func<Task> act = async () => await handler.Handle(new ActivateProductCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
