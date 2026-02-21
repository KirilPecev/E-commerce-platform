using ECommercePlatform.Application.Interfaces;

using FluentAssertions;

using InventoryService.Application.Inventory.Commands;
using InventoryService.Domain.Aggregates;
using InventoryService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace InventoryService.Tests.ApplicationTests
{
    public class ConfirmStockCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ConfirmsPendingReservationsForOrder()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new InventoryDbContext(options, dispatcherMock.Object))
            {
                var orderId = Guid.NewGuid();

                var res1 = new StockReservation(orderId, 3);
                var res2 = new StockReservation(orderId, 2);
                var other = new StockReservation(Guid.NewGuid(), 5);

                context.StockReservations.AddRange(res1, res2, other);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                var handler = new ConfirmStockCommandHandler(context);

                await handler.Handle(new ConfirmStockCommand(orderId), CancellationToken.None);

                var reservations = await context.StockReservations.Where(r => r.OrderId == orderId).ToListAsync(TestContext.Current.CancellationToken);

                reservations.Should().OnlyContain(r => r.Status == ReservationStatus.Confirmed);

                var otherReservation = await context.StockReservations.FirstAsync(r => r.OrderId == other.OrderId, TestContext.Current.CancellationToken);
                otherReservation.Status.Should().Be(ReservationStatus.Pending);
            }
        }

        [Fact]
        public async Task Handle_NoPendingReservations_NoChanges()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new InventoryDbContext(options, dispatcherMock.Object))
            {
                var orderId = Guid.NewGuid();

                var confirmed = new StockReservation(orderId, 1);
                confirmed.Confirm();

                context.StockReservations.Add(confirmed);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                var handler = new ConfirmStockCommandHandler(context);

                // should not throw
                await handler.Handle(new ConfirmStockCommand(orderId), CancellationToken.None);

                var saved = await context.StockReservations.FirstAsync(r => r.OrderId == orderId, TestContext.Current.CancellationToken);
                saved.Status.Should().Be(ReservationStatus.Confirmed);
            }
        }
    }
}
