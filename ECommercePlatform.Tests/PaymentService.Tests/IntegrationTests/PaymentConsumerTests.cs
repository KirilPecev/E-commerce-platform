using ECommercePlatform.Events.OrderIntegrationEvents;

using FluentAssertions;

using MassTransit;
using MassTransit.Testing;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PaymentService.Domain.Aggregates;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Tests.IntegrationTests
{
    public class PaymentConsumerTests : IClassFixture<PaymentWebApplicationFactory>
    {
        [Fact]
        public async Task OrderFinalizedEvent_ShouldCreatePayment()
        {
            // Arrange
            var factory = new PaymentWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var orderId = Guid.NewGuid();

            using var scope = factory.Services.CreateScope();
            var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            // Act
            await harness.Bus.Publish(new OrderFinalizedIntegrationEvent
            {
                OrderId = orderId,
                Amount = 250.00m,
                Currecy = "USD",
                OccurredOn = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);

            await harness.InactivityTask;

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            var payment = await db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId, TestContext.Current.CancellationToken);

            payment.Should().NotBeNull();
            payment!.Amount.Amount.Should().Be(250.00m);
            payment.Amount.Currency.Should().Be("USD");
            payment.Status.Should().Be(PaymentStatus.Pending);
        }
    }
}
