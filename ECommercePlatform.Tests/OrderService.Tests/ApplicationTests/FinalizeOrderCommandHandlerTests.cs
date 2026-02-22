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
    public class FinalizeOrderCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenOrderNotFound()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var handler = new FinalizeOrderCommandHandler(context);

            Func<Task> act = () => handler.Handle(new FinalizeOrderCommand(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_Throws_WhenNoItems()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var order = new Order(Guid.NewGuid());
            context.Orders.Add(order);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new FinalizeOrderCommandHandler(context);

            Func<Task> act = () => handler.Handle(new FinalizeOrderCommand(order.Id), CancellationToken.None);

            await act.Should().ThrowAsync<OrderDomainException>().WithMessage("Cannot finalize an empty order.");
        }

        [Fact]
        public async Task Handle_Throws_WhenNoAddress()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(5m, "EUR"), 1);
            context.Orders.Add(order);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new FinalizeOrderCommandHandler(context);

            Func<Task> act = () => handler.Handle(new FinalizeOrderCommand(order.Id), CancellationToken.None);

            await act.Should().ThrowAsync<OrderDomainException>().WithMessage("Cannot finalize an order without adress.");
        }

        [Fact]
        public async Task Handle_FinalizesOrderAndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var order = new Order(Guid.NewGuid());
                order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(7.5m, "EUR"), 2); // total 15 EUR
                order.SetShippingAddress(new Address("St", "C", "Z", "Country"));

                context.Orders.Add(order);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                // reset mock to ignore events from initial save (OrderCreated)
                dispatcherMock.Reset();

                var handler = new FinalizeOrderCommandHandler(context);

                await handler.Handle(new FinalizeOrderCommand(order.Id), CancellationToken.None);

                var saved = await context.Orders.FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
                saved.Status.Should().Be(OrderStatus.Finalized);

                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(OrderFinalizedDomainEvent) && ((OrderFinalizedDomainEvent)ev).OrderId == order.Id && ((OrderFinalizedDomainEvent)ev).TotalPrice == order.TotalPrice && ((OrderFinalizedDomainEvent)ev).Currency == "EUR")), Times.Once);
            }
        }
    }
}
