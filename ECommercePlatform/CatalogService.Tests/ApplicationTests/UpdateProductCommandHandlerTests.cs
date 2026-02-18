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
    public class UpdateProductCommandHandlerTests
    {
        private readonly Mock<IProductCache> cacheMock;

        public UpdateProductCommandHandlerTests()
        {
            cacheMock = new Mock<IProductCache>();
        }

        [Fact]
        public async Task Handle_ShouldUpdateProduct_AndInvalidateCache_WhenDataIsValid()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);
            var otherCategory = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-00000000000B"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("Old Name"),
                new Money(100m, "USD"),
                category!,
                "Old desc"
            );

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new UpdateProductCommandHandler(dbContext, cacheMock.Object);

            var command = new UpdateProductCommand(product.Id, "New Name", 120m, "EUR", otherCategory!.Id, "New desc");

            await handler.Handle(command, TestContext.Current.CancellationToken);

            var updated = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id, TestContext.Current.CancellationToken);

            updated.Should().NotBeNull();
            updated!.Name.Value.Should().Be("New Name");
            updated.Price.Amount.Should().Be(120m);
            updated.Price.Currency.Should().Be("EUR");
            updated.Category.Id.Should().Be(otherCategory!.Id);
            updated.Description.Should().Be("New desc");

            cacheMock.Verify(x => x.RemoveByIdAsync(product.Id), Times.Once);
            cacheMock.Verify(x => x.RemoveAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenProductDoesNotExist()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new UpdateProductCommandHandler(dbContext, cacheMock.Object);

            var command = new UpdateProductCommand(Guid.NewGuid(), "Name", 10m, "USD", Guid.NewGuid(), "Desc");

            Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenCategoryDoesNotExist()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            Product product = new Product(
                new ProductName("Old Name"),
                new Money(100m, "USD"),
                category!,
                "Old desc"
            );

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new UpdateProductCommandHandler(dbContext, cacheMock.Object);

            var command = new UpdateProductCommand(product.Id, "New Name", 120m, "EUR", Guid.NewGuid(), "New desc");

            Func<Task> act = async () => await handler.Handle(command, TestContext.Current.CancellationToken);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
