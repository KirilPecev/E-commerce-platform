using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

using ECommercePlatform.Application.Interfaces;
using OrderService.Application.Orders.Commands;
using OrderService.Domain.Events;
using ECommercePlatform.Domain.Events;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Tests.ApplicationTests
{
    public class CreateOrderCommandHandlerTests
    {
        [Fact]
        public async Task Handle_CreatesOrder_PersistsAndDispatchesOrderCreatedEvent()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var handler = new CreateOrderCommandHandler(context);

            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            var cmd = new CreateOrderCommand(customerId, productId, variantId, "Product X", 12.5m, "USD", 3);

            var orderId = await handler.Handle(cmd, CancellationToken.None);

            orderId.Should().NotBe(Guid.Empty);

            var saved = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
            saved.Should().NotBeNull();
            saved!.CustomerId.Should().Be(customerId);
            saved.Items.Should().HaveCount(1);
            saved.TotalPrice.Should().Be(12.5m * 3);

            dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(OrderCreatedDomainEvent) && ((OrderCreatedDomainEvent)ev).ProductId == productId && ((OrderCreatedDomainEvent)ev).ProductVariantId == variantId && ((OrderCreatedDomainEvent)ev).Quantity == 3)), Times.Once);
        }
    }
}
