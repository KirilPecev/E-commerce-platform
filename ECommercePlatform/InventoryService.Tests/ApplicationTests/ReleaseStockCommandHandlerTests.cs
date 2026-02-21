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
    public class ReleaseStockCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenStockNotFound()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>(TestContext.Current.CancellationToken);

            await using var context = new InventoryDbContext(options, dispatcherMock.Object);

            var handler = new ReleaseStockCommandHandler(context);

            Func<Task> act = () => handler.Handle(new ReleaseStockCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Stock not found.");
        }

        [Fact]
        public async Task Handle_Throws_WhenReservationNotFound()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>(TestContext.Current.CancellationToken);

            await using (var context = new InventoryDbContext(options, dispatcherMock.Object))
            {
                var productId = Guid.NewGuid();
                var variantId = Guid.NewGuid();

                var stock = new ProductStock(productId, variantId, 10);
                // reserve for a different order
                stock.Reserve(Guid.NewGuid(), 2);

                context.ProductStocks.Add(stock);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                var handler = new ReleaseStockCommandHandler(context);

                Func<Task> act = () => handler.Handle(new ReleaseStockCommand(productId, variantId, Guid.NewGuid()), CancellationToken.None);

                await act.Should().ThrowAsync<InventoryDomainException>().WithMessage("Reservation not found.");
            }
        }

        [Fact]
        public async Task Handle_ReleasesReservationAndRestoresQuantity_AndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>(TestContext.Current.CancellationToken);

            await using (var context = new InventoryDbContext(options, dispatcherMock.Object))
            {
                var productId = Guid.NewGuid();
                var variantId = Guid.NewGuid();
                var orderId = Guid.NewGuid();

                var stock = new ProductStock(productId, variantId, 10);
                stock.Reserve(orderId, 4); // available becomes 6

                context.ProductStocks.Add(stock);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                // Clear previous invocations (Reserve triggered an event on initial save)
                dispatcherMock.Reset();

                var handler = new ReleaseStockCommandHandler(context);

                await handler.Handle(new ReleaseStockCommand(productId, variantId, orderId), CancellationToken.None);

                // Reload from DB to ensure changes persisted
                var saved = await context.ProductStocks.Include(ps => ps.Reservations).FirstAsync(ps => ps.ProductId == productId && ps.ProductVariantId == variantId, TestContext.Current.CancellationToken);

                saved.AvailableQuantity.Should().Be(10);

                var reservation = saved.Reservations.First(r => r.OrderId == orderId);
                reservation.Status.Should().Be(ReservationStatus.Released);

                // Ensure StockReleasedDomainEvent was dispatched
                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(StockReleasedDomainEvent) && ((StockReleasedDomainEvent)ev).OrderId == orderId && ((StockReleasedDomainEvent)ev).ProductId == productId && ((StockReleasedDomainEvent)ev).ProductVariantId == variantId)), Times.Once);
            }
        }
    }
}
