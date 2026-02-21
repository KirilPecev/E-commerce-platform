using ECommercePlatform.Application.Interfaces;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using OrderService.Application.Orders.Queries;
using OrderService.Domain.Aggregates;
using OrderService.Domain.ValueObjects;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Tests.ApplicationTests
{
    public class GetOrdersByCustomerQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoOrdersForCustomer()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var handler = new GetOrdersByCustomerQueryHandler(context);

            var result = await handler.Handle(new GetOrdersByCustomerQuery(Guid.NewGuid()), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsOrdersForCustomer_WithCorrectDtos()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var customerA = Guid.NewGuid();
                var customerB = Guid.NewGuid();

                var order1 = new Order(customerA);
                order1.AddItem(Guid.NewGuid(), Guid.NewGuid(), "A1", new Money(10m, "USD"), 1);
                order1.SetShippingAddress(new Address("St1", "C1", "Z1", "Country"));
                order1.FinalizeOrder();

                var order2 = new Order(customerA);
                order2.AddItem(Guid.NewGuid(), Guid.NewGuid(), "A2", new Money(5m, "USD"), 2);
                order2.SetShippingAddress(new Address("St2", "C2", "Z2", "Country"));
                order2.FinalizeOrder();

                var other = new Order(customerB);
                other.AddItem(Guid.NewGuid(), Guid.NewGuid(), "B1", new Money(3m, "USD"), 1);
                other.SetShippingAddress(new Address("St3", "C3", "Z3", "Country"));
                other.FinalizeOrder();

                context.Orders.AddRange(order1, order2, other);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                // reset dispatcher
                dispatcherMock.Reset();

                var handler = new GetOrdersByCustomerQueryHandler(context);

                var result = await handler.Handle(new GetOrdersByCustomerQuery(customerA), CancellationToken.None);

                result.Should().HaveCount(2);

                result.Select(r => r.Id).Should().Contain(new[] { order1.Id, order2.Id });

                var dto1 = result.Single(r => r.Id == order1.Id);
                dto1.Items.Should().HaveCount(1);
                dto1.TotalPrice.Should().Be(order1.TotalPrice);
                dto1.ShippingAddress.Should().NotBeNull();
                dto1.ShippingAddress!.Street.Should().Be("St1");

                var dto2 = result.Single(r => r.Id == order2.Id);
                dto2.Items.Should().HaveCount(1);
                dto2.TotalPrice.Should().Be(order2.TotalPrice);
                dto2.ShippingAddress.Should().NotBeNull();
                dto2.ShippingAddress!.Street.Should().Be("St2");
            }
        }
    }
}
