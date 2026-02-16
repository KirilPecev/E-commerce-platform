using CatalogService.Application.Interfaces;
using CatalogService.Application.Products.Commands;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.ProductIntegrationEvents;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace CatalogService.Tests.ApplicationTests
{
    public class CreateProductCommandHandlerTests
    {
        private readonly Mock<IEventPublisher> eventPublisherMock;
        private readonly Mock<IProductCache> cacheMock;

        public CreateProductCommandHandlerTests()
        {
            eventPublisherMock = new Mock<IEventPublisher>();
            cacheMock = new Mock<IProductCache>();
        }

        [Fact]
        public async Task Handle_ShouldCreateProductAndPublishEvent_AndInvalidateCache()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new CreateProductCommandHandler(
                dbContext,
                cacheMock!.Object
            );

            var command = new CreateProductCommand(
                "Laptop",
                1500,
                "USD",
                Guid.Parse("11111111-0000-0000-0000-000000000001"),
                "Gaming laptop"
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == result, TestContext.Current.CancellationToken);

            Assert.NotNull(product);
            product.Name.Value.Should().Be("Laptop");
            product.Price.Amount.Should().Be(1500);
            product.Price.Currency.Should().Be("USD");
            product.Category.Id.Should().Be(command.CategoryId);
            product.Description.Should().Be("Gaming laptop");

            // Assert — cache invalidation
            cacheMock.Verify(
                x => x.RemoveAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenCategoryIsInvalid()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new CreateProductCommandHandler(
                dbContext,
                cacheMock!.Object
            );

            var command = new CreateProductCommand(
                "Laptop",
                1500,
                "USD",
                Guid.NewGuid(),
                "Gaming laptop"
            );

            // Act
            Func<Task> action = async () => await handler.Handle(command, CancellationToken.None);

            await action.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
