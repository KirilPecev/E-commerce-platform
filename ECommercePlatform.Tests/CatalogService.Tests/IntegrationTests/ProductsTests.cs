using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using CatalogService.Contracts.Responses;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;

namespace CatalogService.Tests.IntegrationTests
{
    public class ProductsTests : IClassFixture<CatalogWebApplicationFactory>
    {
        [Fact]
        public async Task Create_ShouldReturnCreated_WhenAdmin()
        {
            // Arrange
            var (client, categoryId) = await CreateClientWithCategoryAsync();

            // Act
            var response = await client.PostAsJsonAsync("/api/products", new
            {
                Name = "Test Product",
                Amount = 99.99m,
                Currency = "USD",
                CategoryId = categoryId,
                Description = "A test product"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Create_ShouldReturnForbidden_WhenCustomer()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.PostAsJsonAsync("/api/products", new
            {
                Name = "Test Product",
                Amount = 99.99m,
                Currency = "USD",
                CategoryId = Guid.NewGuid(),
                Description = "A test product"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Create_ShouldReturnUnauthorized_WhenNoToken()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/products", new
            {
                Name = "Test Product",
                Amount = 99.99m,
                Currency = "USD",
                CategoryId = Guid.NewGuid(),
                Description = "A test product"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenCategoryNotFound()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            // Act
            Func<Task> act = async () => await client.PostAsJsonAsync("/api/products", new
            {
                Name = "Test Product",
                Amount = 99.99m,
                Currency = "USD",
                CategoryId = Guid.NewGuid(),
                Description = "A test product"
            },
            TestContext.Current.CancellationToken);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetById_ShouldReturnProduct()
        {
            // Arrange
            var (client, categoryId) = await CreateClientWithCategoryAsync();

            var createResponse = await client.PostAsJsonAsync("/api/products", new
            {
                Name = "Laptop",
                Amount = 999.99m,
                Currency = "USD",
                CategoryId = categoryId,
                Description = "A powerful laptop"
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync(
                $"/api/products/{created!.Id}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var product = await response.Content.ReadFromJsonAsync<ProductResponse>(TestContext.Current.CancellationToken);

            product.Should().NotBeNull();
            product!.Name.Should().Be("Laptop");
            product.Amount.Should().Be(999.99m);
            product.Currency.Should().Be("USD");
            product.CategoryId.Should().Be(categoryId);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync(
                $"/api/products/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAll_ShouldReturnProducts()
        {
            // Arrange
            var (client, categoryId) = await CreateClientWithCategoryAsync();

            await client.PostAsJsonAsync("/api/products", new
            {
                Name = "Product A",
                Amount = 10.00m,
                Currency = "USD",
                CategoryId = categoryId
            },
            TestContext.Current.CancellationToken);

            await client.PostAsJsonAsync("/api/products", new
            {
                Name = "Product B",
                Amount = 20.00m,
                Currency = "EUR",
                CategoryId = categoryId
            },
            TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync("/api/products", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var products = await response.Content
                .ReadFromJsonAsync<List<ProductResponse>>(TestContext.Current.CancellationToken);

            products.Should().NotBeNull();
            products!.Count.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task Update_ShouldReturnCreatedAtAction_WhenAdmin()
        {
            // Arrange
            var (client, categoryId) = await CreateClientWithCategoryAsync();

            var createResponse = await client.PostAsJsonAsync("/api/products", new
            {
                Name = "Old Product",
                Amount = 50.00m,
                Currency = "USD",
                CategoryId = categoryId
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            // Act
            var updateResponse = await client.PutAsJsonAsync($"/api/products/{created!.Id}", new
            {
                Name = "Updated Product",
                Amount = 75.00m,
                Currency = "EUR",
                CategoryId = categoryId,
                Description = "Updated description"
            },
            TestContext.Current.CancellationToken);

            // Assert
            updateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var getResponse = await client.GetAsync(
                $"/api/products/{created.Id}", TestContext.Current.CancellationToken);

            var product = await getResponse.Content.ReadFromJsonAsync<ProductResponse>(TestContext.Current.CancellationToken);

            product!.Name.Should().Be("Updated Product");
            product.Amount.Should().Be(75.00m);
            product.Currency.Should().Be("EUR");
        }

        [Fact]
        public async Task Deactivate_ShouldReturnNoContent_WhenAdmin()
        {
            // Arrange
            var (client, categoryId) = await CreateClientWithCategoryAsync();

            var createResponse = await client.PostAsJsonAsync("/api/products", new
            {
                Name = "To Deactivate",
                Amount = 30.00m,
                Currency = "USD",
                CategoryId = categoryId
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            // Act
            var deleteResponse = await client.DeleteAsync(
                $"/api/products/{created!.Id}", TestContext.Current.CancellationToken);

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Activate_ShouldReturnNoContent_WhenAdmin()
        {
            // Arrange
            var (client, categoryId) = await CreateClientWithCategoryAsync();

            var createResponse = await client.PostAsJsonAsync("/api/products", new
            {
                Name = "To Activate",
                Amount = 30.00m,
                Currency = "USD",
                CategoryId = categoryId
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            await client.DeleteAsync(
                $"/api/products/{created!.Id}", TestContext.Current.CancellationToken);

            // Act
            var activateResponse = await client.PostAsync(
                $"/api/products/{created.Id}/activate", null, TestContext.Current.CancellationToken);

            // Assert
            activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Deactivate_ShouldReturnForbidden_WhenCustomer()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.DeleteAsync(
                $"/api/products/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private static async Task<(HttpClient Client, Guid CategoryId)> CreateClientWithCategoryAsync()
        {
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            var categoryResponse = await client.PostAsJsonAsync("/api/categories", new
            {
                Name = $"TestCategory-{Guid.NewGuid():N}",
                Description = "Test category for products"
            },
            TestContext.Current.CancellationToken);

            var created = await categoryResponse.Content.ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            return (client, created!.Id);
        }

        private record CreatedResponse(Guid Id);
    }
}
