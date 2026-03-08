using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using InventoryService.Application.Inventory.Queries;
using InventoryService.Domain.Aggregates;
using InventoryService.Infrastructure.Persistence;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Tests.IntegrationTests
{
    public class InventoryTests : IClassFixture<InventoryWebApplicationFactory>
    {
        [Fact]
        public async Task GetStocks_ShouldReturnOk_WhenAdmin()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                db.ProductStocks.Add(new ProductStock(productId, variantId, 50));
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", InventoryTestTokenGenerator.GenerateAdminToken());

            // Act
            var response = await client.GetAsync(
                $"/api/inventory/{productId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var stocks = await response.Content
                .ReadFromJsonAsync<List<ProductStockDto>>(TestContext.Current.CancellationToken);

            stocks.Should().NotBeNull();
            stocks!.Should().ContainSingle();
            stocks[0].ProductId.Should().Be(productId);
            stocks[0].ProductVariantId.Should().Be(variantId);
            stocks[0].QuantityAvailable.Should().Be(50);
            stocks[0].QuantityReserved.Should().Be(0);
        }

        [Fact]
        public async Task GetStocks_ShouldReturnOk_WhenCustomer()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                db.ProductStocks.Add(new ProductStock(productId, variantId, 25));
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", InventoryTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.GetAsync(
                $"/api/inventory/{productId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var stocks = await response.Content
                .ReadFromJsonAsync<List<ProductStockDto>>(TestContext.Current.CancellationToken);

            stocks.Should().NotBeNull();
            stocks!.Should().ContainSingle();
            stocks[0].QuantityAvailable.Should().Be(25);
        }

        [Fact]
        public async Task GetStocks_ShouldReturnUnauthorized_WhenNoToken()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync(
                $"/api/inventory/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetStocks_ShouldReturnEmptyList_WhenNoStockExists()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", InventoryTestTokenGenerator.GenerateAdminToken());

            // Act
            var response = await client.GetAsync(
                $"/api/inventory/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var stocks = await response.Content
                .ReadFromJsonAsync<List<ProductStockDto>>(TestContext.Current.CancellationToken);

            stocks.Should().NotBeNull();
            stocks!.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStocks_ShouldReturnMultipleVariants_WhenProductHasMultipleStocks()
        {
            // Arrange
            var factory = new InventoryWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var productId = Guid.NewGuid();

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
                db.ProductStocks.Add(new ProductStock(productId, Guid.NewGuid(), 10));
                db.ProductStocks.Add(new ProductStock(productId, Guid.NewGuid(), 20));
                db.ProductStocks.Add(new ProductStock(productId, Guid.NewGuid(), 30));
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", InventoryTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.GetAsync(
                $"/api/inventory/{productId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var stocks = await response.Content
                .ReadFromJsonAsync<List<ProductStockDto>>(TestContext.Current.CancellationToken);

            stocks.Should().NotBeNull();
            stocks!.Count.Should().Be(3);
        }
    }
}
