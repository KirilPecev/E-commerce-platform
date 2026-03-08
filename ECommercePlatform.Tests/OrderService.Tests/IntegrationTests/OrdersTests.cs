using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;

using OrderService.Application.Orders.Queries;
using OrderService.Domain.Aggregates;

namespace OrderService.Tests.IntegrationTests
{
    public class OrdersTests : IClassFixture<OrderWebApplicationFactory>
    {
        [Fact]
        public async Task Create_ShouldReturnCreated_WhenCustomer()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Test Product",
                Price = 29.99m,
                Currency = "USD",
                Quantity = 2
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
        {
            // Arrange
            var factory = new OrderWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Test Product",
                Price = 29.99m,
                Currency = "USD",
                Quantity = 2
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_ShouldReturnOrder()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            var customerId = Guid.NewGuid();
            var createResponse = await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = customerId,
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Laptop",
                Price = 999.99m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content
                .ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync(
                $"/api/orders/{created!.Id}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var order = await response.Content
                .ReadFromJsonAsync<OrderDto>(TestContext.Current.CancellationToken);

            order.Should().NotBeNull();
            order!.CustomerId.Should().Be(customerId);
            order.Status.Should().Be(OrderStatus.Draft);
            order.Items.Should().ContainSingle();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            // Act
            var response = await client.GetAsync(
                $"/api/orders/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SetAddress_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            var orderId = await CreateOrderAsync(client);

            // Act
            var response = await client.PutAsJsonAsync($"/api/orders/{orderId}/address", new
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "US"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Finalize_ShouldReturnNoContent_WhenOrderIsComplete()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            var orderId = await CreateOrderAsync(client);

            await client.PutAsJsonAsync($"/api/orders/{orderId}/address", new
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "US"
            },
            TestContext.Current.CancellationToken);

            // Act
            var response = await client.PostAsync(
                $"/api/orders/{orderId}/finalize", null, TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Finalize_ShouldThrow_WhenNoAddress()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            var orderId = await CreateOrderAsync(client);

            // Act
            Func<Task> act = async () => await client.PostAsync(
                $"/api/orders/{orderId}/finalize", null, TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Cancel_ShouldReturnNoContent()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            var orderId = await CreateOrderAsync(client);

            // Act
            var response = await client.PostAsJsonAsync($"/api/orders/{orderId}/cancel", new
            {
                Reason = "Changed my mind"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Cancel_ShouldThrow_WhenNoReason()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            var orderId = await CreateOrderAsync(client);

            // Act
            Func<Task> act = async () => await client.PostAsJsonAsync($"/api/orders/{orderId}/cancel", new
            {
                Reason = ""
            },
            TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Ship_ShouldReturnForbidden_WhenCustomer()
        {
            // Arrange
            var factory = new OrderWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", OrderTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.PostAsJsonAsync($"/api/orders/{Guid.NewGuid()}/ship", new
            {
                TrackingNumber = "TRACK123"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Ship_ShouldThrow_WhenOrderNotPaid()
        {
            // Arrange
            var client = CreateAuthenticatedClient("Admin");

            var orderId = await CreateOrderAsync(client);

            await client.PutAsJsonAsync($"/api/orders/{orderId}/address", new
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "US"
            },
            TestContext.Current.CancellationToken);

            await client.PostAsync(
                $"/api/orders/{orderId}/finalize", null, TestContext.Current.CancellationToken);

            // Act — order is Finalized but not Paid
            Func<Task> act = async () => await client.PostAsJsonAsync($"/api/orders/{orderId}/ship", new
            {
                TrackingNumber = "TRACK123"
            },
            TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetAllOrdersForCustomer_ShouldReturnOrders()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            var customerId = Guid.NewGuid();

            await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = customerId,
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Product A",
                Price = 10.00m,
                Currency = "USD",
                Quantity = 1
            },
            TestContext.Current.CancellationToken);

            await client.PostAsJsonAsync("/api/orders", new
            {
                CustomerId = customerId,
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                ProductName = "Product B",
                Price = 20.00m,
                Currency = "USD",
                Quantity = 2
            },
            TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync(
                $"/api/orders/customer/{customerId}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var orders = await response.Content
                .ReadFromJsonAsync<List<OrderDto>>(TestContext.Current.CancellationToken);

            orders.Should().NotBeNull();
            orders!.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetAllOrdersForCustomer_ShouldReturnEmptyList_WhenNoOrders()
        {
            // Arrange
            var client = CreateAuthenticatedClient();

            // Act
            var response = await client.GetAsync(
                $"/api/orders/customer/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var orders = await response.Content
                .ReadFromJsonAsync<List<OrderDto>>(TestContext.Current.CancellationToken);

            orders.Should().NotBeNull();
            orders!.Should().BeEmpty();
        }

        private static HttpClient CreateAuthenticatedClient(string role = "Customer")
        {
            var factory = new OrderWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();

            var token = role == "Admin"
                ? OrderTestTokenGenerator.GenerateAdminToken()
                : OrderTestTokenGenerator.GenerateCustomerToken();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

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
