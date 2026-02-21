using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using InventoryService.Application.Inventory.Commands;
using InventoryService.Domain.Aggregates;
using InventoryService.Domain.Events;
using InventoryService.Domain.Exceptions;
using InventoryService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace InventoryService.Tests.ApplicationTests
{
    public class ReserveStockCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenStockNotFound()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new InventoryDbContext(options, dispatcherMock.Object);

            var handler = new ReserveStockCommandHandler(context);

            Func<Task> act = () => handler.Handle(new ReserveStockCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Stock not found.");
        }

        [Fact]
        public async Task Handle_Throws_WhenQuantityNotPositive()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new InventoryDbContext(options, dispatcherMock.Object);

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var stock = new ProductStock(productId, variantId, 5);

            context.ProductStocks.Add(stock);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new ReserveStockCommandHandler(context);

            Func<Task> act = () => handler.Handle(new ReserveStockCommand(productId, variantId, Guid.NewGuid(), 0), CancellationToken.None);

            await act.Should().ThrowAsync<InventoryDomainException>().WithMessage("Quantity must be positive.");
        }

        [Fact]
        public async Task Handle_Fails_WhenInsufficientAvailable_DispatchesFailedEvent()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new InventoryDbContext(options, dispatcherMock.Object))
            {
                var productId = Guid.NewGuid();
                var variantId = Guid.NewGuid();
                var orderId = Guid.NewGuid();

                var stock = new ProductStock(productId, variantId, 2);

                context.ProductStocks.Add(stock);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                // reset mock to ignore any events from initial save
                dispatcherMock.Reset();

                var handler = new ReserveStockCommandHandler(context);

                await handler.Handle(new ReserveStockCommand(productId, variantId, orderId, 5), CancellationToken.None);

                var saved = await context.ProductStocks.Include(ps => ps.Reservations).FirstAsync(ps => ps.ProductId == productId && ps.ProductVariantId == variantId, TestContext.Current.CancellationToken);

                saved.AvailableQuantity.Should().Be(2);
                saved.Reservations.Should().BeEmpty();

                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(StockReservationFailedDomainEvent) && ((StockReservationFailedDomainEvent)ev).OrderId == orderId && ((StockReservationFailedDomainEvent)ev).ProductId == productId && ((StockReservationFailedDomainEvent)ev).ProductVariantId == variantId)), Times.Once);
            }
        }

        [Fact]
        public async Task Handle_Succeeds_WhenSufficientAvailable_DispatchesReservedEvent()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new InventoryDbContext(options, dispatcherMock.Object))
            {
                var productId = Guid.NewGuid();
                var variantId = Guid.NewGuid();
                var orderId = Guid.NewGuid();

                var stock = new ProductStock(productId, variantId, 10);

                context.ProductStocks.Add(stock);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                dispatcherMock.Reset();

                var handler = new ReserveStockCommandHandler(context);

                await handler.Handle(new ReserveStockCommand(productId, variantId, orderId, 4), CancellationToken.None);

                var saved = await context.ProductStocks.Include(ps => ps.Reservations).FirstAsync(ps => ps.ProductId == productId && ps.ProductVariantId == variantId, TestContext.Current.CancellationToken);

                saved.AvailableQuantity.Should().Be(6);
                saved.Reservations.Should().HaveCount(1);
                saved.Reservations.First().OrderId.Should().Be(orderId);
                saved.Reservations.First().Quantity.Should().Be(4);

                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(StockReservedDomainEvent) && ((StockReservedDomainEvent)ev).OrderId == orderId && ((StockReservedDomainEvent)ev).ProductId == productId && ((StockReservedDomainEvent)ev).ProductVariantId == variantId && ((StockReservedDomainEvent)ev).Quantity == 4)), Times.Once);
            }
        }
    }
}
