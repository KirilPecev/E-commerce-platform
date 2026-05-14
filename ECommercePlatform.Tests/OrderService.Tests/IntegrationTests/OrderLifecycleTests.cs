using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using ECommercePlatform.Events.PaymentIntegrationEvents;

using FluentAssertions;

using MassTransit.Testing;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using OrderService.Application.Orders.Queries;
using OrderService.Domain.Aggregates;

namespace OrderService.Tests.IntegrationTests
{
    public class OrderLifecycleTests : IClassFixture<OrderWebApplicationFactory>
    {
        [Fact]
        public async Task FullLifecycle_Create_AddItems_SetAddress_Finalize_Pay_Ship()
        {
            // Arrange
            var factory = new OrderWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", OrderTestTokenGenerator.GenerateAdminToken());

            var customerId = Guid.NewGuid();
            var variantId1 = Guid.NewGuid();
            var variantId2 = Guid.NewGuid();

            // === Step 1: Create order ===
            var createResponse = await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = customerId,
                ProductId = Guid.NewGuid(),
                ProductVariantId = variantId1,
                ProductName = "Laptop",
                Price = 999.99m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content
                .ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            var orderId = created!.Id;

            // Verify Draft status
            var order = await GetOrderAsync(client, orderId);
            order.Status.Should().Be(OrderStatus.Draft);
            order.Items.Should().ContainSingle();
            order.TotalPrice.Should().Be(999.99m);

            // === Step 2: Add another item ===
            var addItemResponse = await client.PostAsJsonAsync($"/api/orders/{orderId}/items", new
            {
                ProductId = Guid.NewGuid(),
                ProductVariantId = variantId2,
                ProductName = "Mouse",
                Price = 49.99m,
                Currency = "USD",
                Quantity = 2
            },
            TestContext.Current.CancellationToken);

            addItemResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            order = await GetOrderAsync(client, orderId);
            order.Items.Count.Should().Be(2);
            order.TotalPrice.Should().Be(999.99m + (49.99m * 2));

            // === Step 3: Set shipping address ===
            var addressResponse = await client.PutAsJsonAsync($"/api/orders/{orderId}/address", new
            {
                Street = "742 Evergreen Terrace",
                City = "Springfield",
                State = "IL",
                ZipCode = "62704",
                Country = "US"
            },
            TestContext.Current.CancellationToken);

            addressResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // === Step 4: Finalize ===
            var finalizeResponse = await client.PostAsync(
                $"/api/orders/{orderId}/finalize", null, TestContext.Current.CancellationToken);

            finalizeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            order = await GetOrderAsync(client, orderId);
            order.Status.Should().Be(OrderStatus.Finalized);

            // === Step 5: Simulate payment via consumer ===
            using var scope = factory.Services.CreateScope();
            var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
            await harness.Start();

            await harness.Bus.Publish(new PaymentCompletedIntegrationEvent
            {
                PaymentId = Guid.NewGuid(),
                OrderId = orderId,
                OccurredOn = DateTime.UtcNow
            },
            TestContext.Current.CancellationToken);

            await harness.InactivityTask;

            order = await GetOrderAsync(client, orderId);
            order.Status.Should().Be(OrderStatus.Paid);

            // === Step 6: Ship ===
            var shipResponse = await client.PostAsJsonAsync($"/api/orders/{orderId}/ship", new
            {
                TrackingNumber = "TRACK-12345-XYZ"
            },
            TestContext.Current.CancellationToken);

            shipResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            order = await GetOrderAsync(client, orderId);
            order.Status.Should().Be(OrderStatus.Shipped);
            order.TrackingNumber.Should().Be("TRACK-12345-XYZ");
            order.ShippedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task Lifecycle_Create_AddItems_RemoveItem_Finalize()
        {
            // Arrange
            var client = CreateAdminClient();

            var variantId1 = Guid.NewGuid();
            var variantId2 = Guid.NewGuid();

            // Create order with first item
            var createResponse = await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductVariantId = variantId1,
                ProductName = "Keyboard",
                Price = 79.99m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            var orderId = (await createResponse.Content
                .ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken))!.Id;

            // Add second item
            await client.PostAsJsonAsync($"/api/orders/{orderId}/items", new
            {
                ProductId = Guid.NewGuid(),
                ProductVariantId = variantId2,
                ProductName = "Monitor",
                Price = 299.99m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            var order = await GetOrderAsync(client, orderId);
            order.Items.Count.Should().Be(2);

            // Remove first item
            var itemToRemove = order.Items.First(i => i.ProductVariantId == variantId1);

            var removeResponse = await client.DeleteAsync(
                $"/api/orders/{orderId}/items/{itemToRemove.Id}", TestContext.Current.CancellationToken);

            removeResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            order = await GetOrderAsync(client, orderId);
            order.Items.Should().ContainSingle();
            order.Items[0].ProductVariantId.Should().Be(variantId2);
            order.TotalPrice.Should().Be(299.99m);

            // Set address and finalize
            await client.PutAsJsonAsync($"/api/orders/{orderId}/address", new
            {
                Street = "456 Oak Ave",
                City = "Chicago",
                State = "IL",
                ZipCode = "60601",
                Country = "US"
            },
            TestContext.Current.CancellationToken);

            var finalizeResponse = await client.PostAsync(
                $"/api/orders/{orderId}/finalize", null, TestContext.Current.CancellationToken);

            finalizeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            order = await GetOrderAsync(client, orderId);
            order.Status.Should().Be(OrderStatus.Finalized);
            order.Items.Should().ContainSingle();
        }

        [Fact]
        public async Task Lifecycle_Create_Cancel_VerifyState()
        {
            // Arrange
            var client = CreateAdminClient();

            var createResponse = await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Headphones",
                Price = 199.99m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            var orderId = (await createResponse.Content
                .ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken))!.Id;

            // Cancel
            var cancelResponse = await client.PostAsJsonAsync($"/api/orders/{orderId}/cancel", new
            {
                Reason = "Found a better deal"
            },
            TestContext.Current.CancellationToken);

            cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var order = await GetOrderAsync(client, orderId);
            order.Status.Should().Be(OrderStatus.Cancelled);
            order.CancellationReason.Should().Be("Found a better deal");
        }

        [Fact]
        public async Task Lifecycle_Finalize_ShouldFail_WhenNoItems()
        {
            // Arrange — create order, remove its only item, then try to finalize
            var client = CreateAdminClient();

            var createResponse = await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Tablet",
                Price = 399.99m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            var orderId = (await createResponse.Content
                .ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken))!.Id;

            var order = await GetOrderAsync(client, orderId);
            var itemId = order.Items[0].Id;

            // Remove the only item
            await client.DeleteAsync(
                $"/api/orders/{orderId}/items/{itemId}", TestContext.Current.CancellationToken);

            // Set address
            await client.PutAsJsonAsync($"/api/orders/{orderId}/address", new
            {
                Street = "Street",
                City = "City",
                State = "ST",
                ZipCode = "00000",
                Country = "US"
            },
            TestContext.Current.CancellationToken);

            // Try to finalize — should return BadRequest because no items
            var response = await client.PostAsync($"/api/orders/{orderId}/finalize", null, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task Lifecycle_AddItem_ShouldFail_AfterFinalize()
        {
            // Arrange
            var client = CreateAdminClient();

            var createResponse = await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Phone",
                Price = 699.99m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            var orderId = (await createResponse.Content
                .ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken))!.Id;

            await client.PutAsJsonAsync($"/api/orders/{orderId}/address", new
            {
                Street = "Street",
                City = "City",
                State = "ST",
                ZipCode = "00000",
                Country = "US"
            },
            TestContext.Current.CancellationToken);

            await client.PostAsync(
                $"/api/orders/{orderId}/finalize", null, TestContext.Current.CancellationToken);

            // Act — try to add item after finalize
            var addResponse = await client.PostAsJsonAsync($"/api/orders/{orderId}/items", new
            {
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Case",
                Price = 29.99m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            // Assert
            addResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            addResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task Lifecycle_CustomerOrders_ShouldTrackMultipleOrders()
        {
            // Arrange
            var client = CreateAdminClient();
            var customerId = Guid.NewGuid();

            // Create 3 orders for the same customer
            for (int i = 0; i < 3; i++)
            {
                await client.PostAsJsonAsync("/api/orders", new
                {
                    CustomerId = customerId,
                    ProductId = Guid.NewGuid(),
                    ProductVariantId = Guid.NewGuid(),
                    ProductName = $"Product {i}",
                    Price = 10.00m * (i + 1),
                    Currency = "USD",
                    Quantity = 1
                },
                TestContext.Current.CancellationToken);
            }

            // Act
            var response = await client.GetAsync(
                $"/api/orders/customer/{customerId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var orders = await response.Content
                .ReadFromJsonAsync<List<OrderDto>>(TestContext.Current.CancellationToken);

            orders.Should().NotBeNull();
            orders!.Count.Should().Be(3);
            orders.Should().AllSatisfy(o => o.CustomerId.Should().Be(customerId));
            orders.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Draft));
        }

        private static HttpClient CreateAdminClient()
        {
            var factory = new OrderWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", OrderTestTokenGenerator.GenerateAdminToken());

            return client;
        }

        private static async Task<OrderDto> GetOrderAsync(HttpClient client, Guid orderId)
        {
            var response = await client.GetAsync(
                $"/api/orders/{orderId}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var order = await response.Content
                .ReadFromJsonAsync<OrderDto>(TestContext.Current.CancellationToken);

            order.Should().NotBeNull();

            return order!;
        }

        private record CreatedResponse(Guid Id);
    }
}
