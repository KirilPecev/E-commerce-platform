using CatalogService.Application.Exceptions;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Products.Commands;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace CatalogService.Tests.ApplicationTests
{
    public class DeactivateProductCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldDeactivateProduct_AndInvalidateCache_WhenProductExists()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var cacheMock = new Mock<IProductCache>();

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("Tablet"),
                new Money(250m, "USD"),
                category!,
                "Android tablet"
            );

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new DeactivateProductCommandHandler(dbContext, cacheMock.Object);

            await handler.Handle(new DeactivateProductCommand(product.Id), TestContext.Current.CancellationToken);

            var updated = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id, TestContext.Current.CancellationToken);

            updated.Should().NotBeNull();
            updated!.Status.Should().Be(ProductStatus.Inactive);

            cacheMock.Verify(x => x.RemoveByIdAsync(product.Id), Times.Once);
            cacheMock.Verify(x => x.RemoveAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenProductDoesNotExist()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var cacheMock = new Mock<IProductCache>();

            var handler = new DeactivateProductCommandHandler(dbContext, cacheMock.Object);

            Func<Task> act = async () => await handler.Handle(new DeactivateProductCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
