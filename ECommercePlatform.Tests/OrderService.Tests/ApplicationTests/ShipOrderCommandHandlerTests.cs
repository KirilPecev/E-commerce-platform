using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using OrderService.Application.Orders.Commands;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Events;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Tests.ApplicationTests
{
    public class ShipOrderCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenOrderNotFound()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var handler = new ShipOrderCommandHandler(context);

            Func<Task> act = () => handler.Handle(new ShipOrderCommand(Guid.NewGuid(), "TN"), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_Throws_WhenOrderNotPaid()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);
            order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
            order.FinalizeOrder(); // not paid

            context.Orders.Add(order);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new ShipOrderCommandHandler(context);

            Func<Task> act = () => handler.Handle(new ShipOrderCommand(order.Id, "TN123"), CancellationToken.None);

            await act.Should().ThrowAsync<OrderDomainException>().WithMessage("Only paid orders can be shipped.");
        }

        [Fact]
        public async Task Handle_Throws_WhenTrackingNumberInvalid()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);
            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);
            order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
            order.FinalizeOrder();
            order.MarkAsPaid();

            context.Orders.Add(order);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new ShipOrderCommandHandler(context);

            Func<Task> act = () => handler.Handle(new ShipOrderCommand(order.Id, ""), CancellationToken.None);

            await act.Should().ThrowAsync<OrderDomainException>().WithMessage("Tracking number is required.");
        }

        [Fact]
        public async Task Handle_ShipsOrderAndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var order = new Order(Guid.NewGuid());
                order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(2m, "USD"), 1);
                order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
                order.FinalizeOrder();
                order.MarkAsPaid();

                context.Orders.Add(order);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                dispatcherMock.Reset();

                var handler = new ShipOrderCommandHandler(context);

                await handler.Handle(new ShipOrderCommand(order.Id, "TRACK123"), CancellationToken.None);

                var saved = await context.Orders.FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
                saved.Status.Should().Be(OrderStatus.Shipped);
                saved.TrackingNumber.Should().Be("TRACK123");
                saved.ShippedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(OrderShippedDomainEvent) && ((OrderShippedDomainEvent)ev).TrackingNumber == "TRACK123")), Times.Once);
            }
        }
    }
}
