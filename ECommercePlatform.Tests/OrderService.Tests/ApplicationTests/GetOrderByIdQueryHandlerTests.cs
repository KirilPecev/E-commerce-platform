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
    public class GetOrderByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsNull_WhenOrderNotFound()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var handler = new GetOrderByIdQueryHandler(context);

            var result = await handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ReturnsOrderDto_WithItemsAndAddress()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var customerId = Guid.NewGuid();
                var order = new Order(customerId);

                var productId = Guid.NewGuid();
                var variant1 = Guid.NewGuid();
                var variant2 = Guid.NewGuid();

                order.AddItem(productId, variant1, "Product A", new Money(10m, "USD"), 2); // total 20
                order.AddItem(productId, variant2, "Product B", new Money(5m, "USD"), 1);  // total 5

                order.SetShippingAddress(new Address("Street", "City", "Zip", "Country"));
                order.FinalizeOrder();

                context.Orders.Add(order);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                // reset dispatcher to ignore any events
                dispatcherMock.Reset();

                var handler = new GetOrderByIdQueryHandler(context);

                var dto = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

                dto.Should().NotBeNull();
                dto!.Id.Should().Be(order.Id);
                dto.CustomerId.Should().Be(customerId);
                dto.TotalPrice.Should().Be(order.TotalPrice);
                dto.Status.Should().Be(order.Status);

                dto.ShippingAddress.Should().NotBeNull();
                dto.ShippingAddress!.Street.Should().Be("Street");

                dto.Items.Should().HaveCount(2);
                dto.Items.Should().ContainSingle(i => i.ProductVariantId == variant1 && i.UnitPrice == 10m && i.Quantity == 2 && i.TotalPrice == 20m);
                dto.Items.Should().ContainSingle(i => i.ProductVariantId == variant2 && i.UnitPrice == 5m && i.Quantity == 1 && i.TotalPrice == 5m);
            }
        }
    }
}