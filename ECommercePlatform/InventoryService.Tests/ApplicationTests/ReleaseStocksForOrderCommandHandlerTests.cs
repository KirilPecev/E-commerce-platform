using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using InventoryService.Application.Inventory.Commands;
using InventoryService.Domain.Aggregates;
using InventoryService.Domain.Events;
using InventoryService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace InventoryService.Tests.ApplicationTests
{
    public class ReleaseStocksForOrderCommandHandlerTests
    {
        [Fact]
        public async Task Handle_NoStocks_NoChanges()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new InventoryDbContext(options, dispatcherMock.Object);

            var handler = new ReleaseStocksForOrderCommandHandler(context);

            // should not throw
            await handler.Handle(new ReleaseStocksForOrderCommand(Guid.NewGuid(), "reason"), CancellationToken.None);

            dispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IDomainEvent>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReleasesReservationsForOrderAndDispatchesEvents()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new InventoryDbContext(options, dispatcherMock.Object))
            {
                var orderId = Guid.NewGuid();

                var productId1 = Guid.NewGuid();
                var variantId1 = Guid.NewGuid();
                var stock1 = new ProductStock(productId1, variantId1, 10);
                stock1.Reserve(orderId, 3); // available 7

                var productId2 = Guid.NewGuid();
                var variantId2 = Guid.NewGuid();
                var stock2 = new ProductStock(productId2, variantId2, 5);
                stock2.Reserve(orderId, 2); // available 3

                // unrelated stock
                var stockOther = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 8);
                stockOther.Reserve(Guid.NewGuid(), 1);

                context.ProductStocks.AddRange(stock1, stock2, stockOther);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                // reset mock to ignore events from initial save
                dispatcherMock.Reset();

                var handler = new ReleaseStocksForOrderCommandHandler(context);

                await handler.Handle(new ReleaseStocksForOrderCommand(orderId, "reason"), CancellationToken.None);

                // reload
                var saved1 = await context.ProductStocks.Include(ps => ps.Reservations).FirstAsync(ps => ps.ProductId == productId1 && ps.ProductVariantId == variantId1, TestContext.Current.CancellationToken);
                var saved2 = await context.ProductStocks.Include(ps => ps.Reservations).FirstAsync(ps => ps.ProductId == productId2 && ps.ProductVariantId == variantId2, TestContext.Current.CancellationToken);
                var savedOther = await context.ProductStocks.Include(ps => ps.Reservations).FirstAsync(ps => ps.ProductId == stockOther.ProductId && ps.ProductVariantId == stockOther.ProductVariantId, TestContext.Current.CancellationToken);

                saved1.AvailableQuantity.Should().Be(10);
                saved2.AvailableQuantity.Should().Be(5);

                saved1.Reservations.First(r => r.OrderId == orderId).Status.Should().Be(ReservationStatus.Released);
                saved2.Reservations.First(r => r.OrderId == orderId).Status.Should().Be(ReservationStatus.Released);

                // other reservation should remain pending
                savedOther.Reservations.First().Status.Should().Be(ReservationStatus.Pending);

                // verify dispatcher invoked for each released reservation
                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(StockReleasedDomainEvent) && ((StockReleasedDomainEvent)ev).OrderId == orderId)), Times.Exactly(2));
            }
        }
    }
}
