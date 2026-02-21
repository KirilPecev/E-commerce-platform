using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using OrderService.Application.Orders.Commands;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Events;
using OrderService.Domain.ValueObjects;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Tests.ApplicationTests
{
    public class AddItemCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenOrderNotFound()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var handler = new AddItemCommandHandler(context);

            Func<Task> act = () => handler.Handle(new AddItemCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "P", 1m, "USD", 1), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_AddsNewItemAndPersists_AndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);
            var order = new Order(Guid.NewGuid());
            context.Orders.Add(order);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // reset dispatcher to ignore events from initial save
            dispatcherMock.Reset();

            var handler = new AddItemCommandHandler(context);

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            await handler.Handle(new AddItemCommand(order.Id, productId, variantId, "Product A", 2m, "USD", 3), CancellationToken.None);

            var savedOrder = await context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
            savedOrder.Items.Should().HaveCount(1);
            savedOrder.TotalPrice.Should().Be(6m);

            // ensure Items dbset contains the item
            var savedItem = await context.Items.FirstAsync(i => i.ProductVariantId == variantId, TestContext.Current.CancellationToken);
            savedItem.Quantity.Should().Be(3);

            dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(OrderCreatedDomainEvent) && ((OrderCreatedDomainEvent)ev).ProductVariantId == variantId && ((OrderCreatedDomainEvent)ev).Quantity == 3)), Times.Once);
        }

        [Fact]
        public async Task Handle_IncreasesExistingItem_WhenPresent_AndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);
            var order = new Order(Guid.NewGuid());
            var (item, created) = order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "X", new Money(5m, "USD"), 2);
            // add same item to context items
            context.Orders.Add(order);
            context.Items.Add(item);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // reset dispatcher calls
            dispatcherMock.Reset();

            var handler = new AddItemCommandHandler(context);

            await handler.Handle(new AddItemCommand(order.Id, item.ProductId, item.ProductVariantId, item.ProductName, item.UnitPrice.Amount, item.UnitPrice.Currency, 3), CancellationToken.None);

            var savedItem = await context.Items.FirstAsync(i => i.Id == item.Id, TestContext.Current.CancellationToken);
            savedItem.Quantity.Should().Be(5); // 2 + 3

            dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(OrderCreatedDomainEvent) && ((OrderCreatedDomainEvent)ev).ProductVariantId == item.ProductVariantId && ((OrderCreatedDomainEvent)ev).Quantity == 3)), Times.Once);
        }
    }
}
