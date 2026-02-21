using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using OrderService.Application.Orders.Commands;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Tests.ApplicationTests
{
    public class PayOrderCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenOrderNotFound()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var handler = new PayOrderCommandHandler(context);

            Func<Task> act = () => handler.Handle(new PayOrderCommand(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_Throws_WhenNotFinalized()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var order = new Order(Guid.NewGuid());
            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);

            context.Orders.Add(order);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new PayOrderCommandHandler(context);

            Func<Task> act = () => handler.Handle(new PayOrderCommand(order.Id), CancellationToken.None);

            await act.Should().ThrowAsync<OrderDomainException>().WithMessage("Only finalized orders can be paid.");
        }

        [Fact]
        public async Task Handle_MarksOrderAsPaid_WhenFinalized()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var order = new Order(Guid.NewGuid());
                order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(10m, "USD"), 1);
                order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
                order.FinalizeOrder();

                context.Orders.Add(order);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                dispatcherMock.Reset();

                var handler = new PayOrderCommandHandler(context);

                await handler.Handle(new PayOrderCommand(order.Id), CancellationToken.None);

                var saved = await context.Orders.FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
                saved.Status.Should().Be(OrderStatus.Paid);

                // MarkAsPaid does not produce domain events, so dispatcher should not be called
                dispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IDomainEvent>()), Times.Never);
            }
        }
    }
}
