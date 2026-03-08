using ECommercePlatform.Events.PaymentIntegrationEvents;

using FluentAssertions;

using MassTransit;
using MassTransit.Testing;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OrderService.Domain.Aggregates;
using OrderService.Domain.ValueObjects;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Tests.IntegrationTests
{
    public class OrderConsumerTests : IClassFixture<OrderWebApplicationFactory>
    {
        [Fact]
        public async Task PaymentCompletedEvent_ShouldMarkOrderAsPaid()
        {
            // Arrange
            var factory = new OrderWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var orderId = Guid.NewGuid();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

            var order = new Order(Guid.NewGuid());

            var idProp = typeof(Order).BaseType!.GetProperty("Id")!;
            idProp.SetValue(order, orderId);

            order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "Test Product", new Money(50.00m, "USD"), 1);
            order.SetShippingAddress(new Address("123 Main St", "Springfield", "62701", "US"));
            order.FinalizeOrder();

            db.Orders.Add(order);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            // Act
            await harness.Bus.Publish(new PaymentCompletedIntegrationEvent
            {
                PaymentId = Guid.NewGuid(),
                OrderId = orderId,
                OccurredOn = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);

            await harness.InactivityTask;

            // Assert
            var updated = await db.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId, TestContext.Current.CancellationToken);

            updated.Should().NotBeNull();
            updated!.Status.Should().Be(OrderStatus.Paid);
        }
    }
}
