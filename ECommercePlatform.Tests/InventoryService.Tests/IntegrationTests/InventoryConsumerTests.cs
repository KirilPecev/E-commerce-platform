using ECommercePlatform.Events.OrderIntegrationEvents;
using ECommercePlatform.Events.ProductIntegrationEvents;

using FluentAssertions;

using InventoryService.Domain.Aggregates;
using InventoryService.Infrastructure.Persistence;

using MassTransit.Testing;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Tests.IntegrationTests
{
    public class InventoryConsumerTests : IClassFixture<InventoryWebApplicationFactory>
    {
        [Fact]
        public async Task ProductCreatedEvent_ShouldCreateProductStock()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            using var scope = factory.Services.CreateScope();
            var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            // Act
            await harness.Bus.Publish(new ProductCreatedIntegrationEvent
            {
                ProductId = productId,
                ProductVariantId = variantId,
                InitialQuantity = 100,
                OccurredOn = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);

            await harness.InactivityTask;

            // Assert
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = await db.ProductStocks
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.ProductVariantId == variantId, TestContext.Current.CancellationToken);

            stock.Should().NotBeNull();
            stock!.AvailableQuantity.Should().Be(100);
        }

        [Fact]
        public async Task ProductUpdatedEvent_ShouldUpdateStockQuantity()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            db.ProductStocks.Add(new ProductStock(productId, variantId, 50));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            // Act
            await harness.Bus.Publish(new ProductUpdatedIntegrationEvent
            {
                ProductId = productId,
                ProductVariantId = variantId,
                Quantity = 200,
                OccurredOn = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);

            await harness.InactivityTask;

            // Assert
            var updated = await db.ProductStocks
                .AsNoTracking()
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.ProductVariantId == variantId, TestContext.Current.CancellationToken);

            updated.Should().NotBeNull();
            updated!.AvailableQuantity.Should().Be(200);
        }

        [Fact]
        public async Task OrderCreatedEvent_ShouldReserveStock()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            db.ProductStocks.Add(new ProductStock(productId, variantId, 50));
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            // Act
            await harness.Bus.Publish(new OrderCreatedIntegrationEvent
            {
                OrderId = orderId,
                ProductId = productId,
                ProductVariantId = variantId,
                Quantity = 5,
                OccurredOn = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);

            await harness.InactivityTask;

            // Assert
            var stock = await db.ProductStocks
                .Include(ps => ps.Reservations)
                .AsNoTracking()
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.ProductVariantId == variantId, TestContext.Current.CancellationToken);

            stock.Should().NotBeNull();
            stock!.Reservations.Should().ContainSingle();
            stock.Reservations[0].OrderId.Should().Be(orderId);
            stock.Reservations[0].Quantity.Should().Be(5);
            stock.Reservations[0].Status.Should().Be(ReservationStatus.Pending);
        }

        [Fact]
        public async Task OrderFinalizedEvent_ShouldConfirmReservations()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = new ProductStock(productId, variantId, 50);
            stock.Reserve(orderId, 10);
            db.ProductStocks.Add(stock);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            // Act
            await harness.Bus.Publish(new OrderFinalizedIntegrationEvent
            {
                OrderId = orderId,
                Amount = 100.00m,
                Currecy = "USD",
                OccurredOn = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);

            await harness.InactivityTask;

            // Assert
            var reservation = await db.StockReservations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OrderId == orderId, TestContext.Current.CancellationToken);

            reservation.Should().NotBeNull();
            reservation!.Status.Should().Be(ReservationStatus.Confirmed);
        }

        [Fact]
        public async Task OrderCancelledEvent_ShouldReleaseReservations()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = new ProductStock(productId, variantId, 50);
            stock.Reserve(orderId, 10);
            db.ProductStocks.Add(stock);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            // Act
            await harness.Bus.Publish(new OrderCancelledIntegrationEvent
            {
                OrderId = orderId,
                Reason = "Customer changed mind",
                OccurredOn = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);

            await harness.InactivityTask;

            // Assert
            var reservation = await db.StockReservations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OrderId == orderId, TestContext.Current.CancellationToken);

            reservation.Should().NotBeNull();
            reservation!.Status.Should().Be(ReservationStatus.Released);
        }

        [Fact]
        public async Task OrderItemRemovedEvent_ShouldReleaseSpecificStock()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = new ProductStock(productId, variantId, 50);
            stock.Reserve(orderId, 5);
            db.ProductStocks.Add(stock);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            // Act
            await harness.Bus.Publish(new OrderItemRemovedIntegrationEvent
            {
                OrderId = orderId,
                ProductId = productId,
                ProductVariantId = variantId,
                OccurredOn = DateTime.UtcNow
            }, TestContext.Current.CancellationToken);

            await harness.InactivityTask;

            // Assert
            var reservation = await db.StockReservations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OrderId == orderId, TestContext.Current.CancellationToken);

            reservation.Should().NotBeNull();
            reservation!.Status.Should().Be(ReservationStatus.Released);
        }
    }
}
