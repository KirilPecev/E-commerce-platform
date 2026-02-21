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
    public class CancelOrderCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenOrderNotFound()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var handler = new CancelOrderCommandHandler(context);

            Func<Task> act = () => handler.Handle(new CancelOrderCommand(Guid.NewGuid(), "reason"), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_CancelsOrder_AndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var order = new Order(Guid.NewGuid());
                order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);

                context.Orders.Add(order);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                dispatcherMock.Reset();

                var handler = new CancelOrderCommandHandler(context);

                await handler.Handle(new CancelOrderCommand(order.Id, "Customer changed mind"), CancellationToken.None);

                var saved = await context.Orders.FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
                saved.Status.Should().Be(OrderStatus.Cancelled);
                saved.CancellationReason.Should().Be("Customer changed mind");

                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(OrderCancelledDomainEvent) && ((OrderCancelledDomainEvent)ev).Reason == "Customer changed mind")), Times.Once);
            }
        }

        [Fact]
        public async Task Handle_Throws_WhenOrderIsShipped()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var order = new Order(Guid.NewGuid());
                order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);
                order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
                order.FinalizeOrder();
                order.MarkAsPaid();
                order.MarkAsShipped("TRACK-1");

                context.Orders.Add(order);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                var handler = new CancelOrderCommandHandler(context);

                Func<Task> act = () => handler.Handle(new CancelOrderCommand(order.Id, "Too late"), CancellationToken.None);

                await act.Should().ThrowAsync<OrderDomainException>().WithMessage("Shipped orders cannot be cancelled.");
            }
        }
    }
}
