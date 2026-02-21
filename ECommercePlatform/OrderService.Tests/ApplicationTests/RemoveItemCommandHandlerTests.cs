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
    public class RemoveItemCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenOrderNotFound()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var handler = new RemoveItemCommandHandler(context);

            Func<Task> act = () => handler.Handle(new RemoveItemCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_RemovesItem_AndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var order = new Order(Guid.NewGuid());
                var (item, created) = order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(3m, "USD"), 2);

                context.Orders.Add(order);
                context.Items.Add(item);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                dispatcherMock.Reset();

                var handler = new RemoveItemCommandHandler(context);

                await handler.Handle(new RemoveItemCommand(order.Id, item.Id), CancellationToken.None);

                var saved = await context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
                saved.Items.Should().NotContain(i => i.Id == item.Id);

                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(OrderItemRemovedDomainEvent) && ((OrderItemRemovedDomainEvent)ev).ProductId == item.ProductId && ((OrderItemRemovedDomainEvent)ev).ProductVariantId == item.ProductVariantId)), Times.Once);
            }
        }

        [Fact]
        public async Task Handle_Throws_WhenNotDraft()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var order = new Order(Guid.NewGuid());
                var (item, created) = order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(3m, "USD"), 2);
                order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
                order.FinalizeOrder();

                context.Orders.Add(order);
                context.Items.Add(item);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                var handler = new RemoveItemCommandHandler(context);

                Func<Task> act = () => handler.Handle(new RemoveItemCommand(order.Id, item.Id), CancellationToken.None);

                await act.Should().ThrowAsync<OrderDomainException>().WithMessage("Cannot modify a finalized order.");
            }
        }
    }
}
