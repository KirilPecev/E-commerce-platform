using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;

using OrderService.Application.Orders.Queries;

namespace OrderService.Tests.IntegrationTests
{
    public class OrderItemsTests : IClassFixture<OrderWebApplicationFactory>
    {
        [Fact]
        public async Task AddItem_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            var orderId = await CreateOrderAsync(client);

            // Act
            var response = await client.PostAsJsonAsync($"/api/orders/{orderId}/items", new
            {
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Extra Item",
                Price = 15.00m,
                Currency = "USD",
                Quantity = 3
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var getResponse = await client.GetAsync(
                $"/api/orders/{orderId}", TestContext.Current.CancellationToken);

            var order = await getResponse.Content
                .ReadFromJsonAsync<OrderDto>(TestContext.Current.CancellationToken);

            order!.Items.Count.Should().Be(2);
        }

        [Fact]
        public async Task AddItem_ShouldThrow_WhenOrderNotFound()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            // Act
            Func<Task> act = async () => await client.PostAsJsonAsync($"/api/orders/{Guid.NewGuid()}/items", new
            {
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Extra Item",
                Price = 15.00m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task AddItem_ShouldIncreaseQuantity_WhenSameVariantAdded()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            var variantId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var createResponse = await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = Guid.NewGuid(),
                ProductId = productId,
                ProductVariantId = variantId,
                ProductName = "Product",
                Price = 10.00m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content
                .ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            // Act — add the same variant again
            await client.PostAsJsonAsync($"/api/orders/{created!.Id}/items", new
            {
                ProductId = productId,
                ProductVariantId = variantId,
                ProductName = "Product",
                Price = 10.00m,
                Currency = "USD",
                Quantity = 2
            },
            TestContext.Current.CancellationToken);

            // Assert — should still be 1 item with increased quantity
            var getResponse = await client.GetAsync(
                $"/api/orders/{created.Id}", TestContext.Current.CancellationToken);

            var order = await getResponse.Content
                .ReadFromJsonAsync<OrderDto>(TestContext.Current.CancellationToken);

            order!.Items.Should().ContainSingle();
            order.Items[0].Quantity.Should().Be(3);
        }

        [Fact]
        public async Task RemoveItem_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            var orderId = await CreateOrderAsync(client);

            var getResponse = await client.GetAsync(
                $"/api/orders/{orderId}", TestContext.Current.CancellationToken);

            var order = await getResponse.Content
                .ReadFromJsonAsync<OrderDto>(TestContext.Current.CancellationToken);

            var itemId = order!.Items[0].Id;

            // Act
            var response = await client.DeleteAsync(
                $"/api/orders/{orderId}/items/{itemId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task RemoveItem_ShouldThrow_WhenOrderNotFound()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            // Act
            Func<Task> act = async () => await client.DeleteAsync(
                $"/api/orders/{Guid.NewGuid()}/items/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task AddItem_ShouldReturnUnauthorized_WhenNoToken()
        {
            // Arrange
            var factory = new OrderWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync($"/api/orders/{Guid.NewGuid()}/items", new
            {
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Item",
                Price = 10.00m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private static HttpClient CreateAuthenticatedClient()
        {
            var factory = new OrderWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", OrderTestTokenGenerator.GenerateCustomerToken());

            return client;
        }

        private static async Task<Guid> CreateOrderAsync(HttpClient client)
        {
            var response = await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Test Product",
                Price = 50.00m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            var created = await response.Content
                .ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            return created!.Id;
        }

        private record CreatedResponse(Guid Id);
    }
}
