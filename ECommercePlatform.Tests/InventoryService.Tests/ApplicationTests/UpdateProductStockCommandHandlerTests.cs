using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using InventoryService.Application.Inventory.Commands;
using InventoryService.Domain.Aggregates;
using InventoryService.Domain.Exceptions;
using InventoryService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace InventoryService.Tests.ApplicationTests
{
    public class UpdateProductStockCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenStockNotFound()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new InventoryDbContext(options, dispatcherMock.Object);

            var handler = new UpdateProductStockCommandHandler(context);

            Func<Task> act = () => handler.Handle(new UpdateProductStockCommand(Guid.NewGuid(), Guid.NewGuid(), 10), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Product stock not found.");
        }

        [Fact]
        public async Task Handle_UpdatesQuantity_WhenValid()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new InventoryDbContext(options, dispatcherMock.Object))
            {
                var productId = Guid.NewGuid();
                var variantId = Guid.NewGuid();

                var stock = new ProductStock(productId, variantId, 5);

                context.ProductStocks.Add(stock);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                // reset dispatcher to ignore any initial events
                dispatcherMock.Reset();

                var handler = new UpdateProductStockCommandHandler(context);

                await handler.Handle(new UpdateProductStockCommand(productId, variantId, 20), CancellationToken.None);

                var saved = await context.ProductStocks.FirstAsync(ps => ps.ProductId == productId && ps.ProductVariantId == variantId, TestContext.Current.CancellationToken);

                saved.AvailableQuantity.Should().Be(20);

                dispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IDomainEvent>()), Times.Never);
            }
        }

        [Fact]
        public async Task Handle_Throws_WhenQuantityNegative()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new InventoryDbContext(options, dispatcherMock.Object))
            {
                var productId = Guid.NewGuid();
                var variantId = Guid.NewGuid();

                var stock = new ProductStock(productId, variantId, 5);

                context.ProductStocks.Add(stock);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                var handler = new UpdateProductStockCommandHandler(context);

                Func<Task> act = () => handler.Handle(new UpdateProductStockCommand(productId, variantId, -1), CancellationToken.None);

                await act.Should().ThrowAsync<InventoryDomainException>().WithMessage("Quantity cannot be negative.");
            }
        }
    }
}
