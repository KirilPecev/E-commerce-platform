using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using CatalogService.Contracts.Responses;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;

namespace CatalogService.Tests.IntegrationTests
{
    public class ProductVariantsTests : IClassFixture<CatalogWebApplicationFactory>
    {
        [Fact]
        public async Task AddVariant_ShouldReturnCreated_WhenAdmin()
        {
            // Arrange
            var (client, productId) = await CreateClientWithProductAsync();

            // Act
            var response = await client.PostAsJsonAsync($"/api/products/{productId}/variants", new
            {
                Sku = "SKU-001",
                Amount = 49.99m,
                Currency = "USD",
                StockQuantity = 100,
                Size = "M",
                Color = "Blue"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task AddVariant_ShouldReturnForbidden_WhenCustomer()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.PostAsJsonAsync($"/api/products/{Guid.NewGuid()}/variants", new
            {
                Sku = "SKU-001",
                Amount = 49.99m,
                Currency = "USD",
                StockQuantity = 100,
                Size = "M",
                Color = "Blue"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetProductVariants_ShouldReturnVariants()
        {
            // Arrange
            var (client, productId) = await CreateClientWithProductAsync();

            await client.PostAsJsonAsync($"/api/products/{productId}/variants", new
            {
                Sku = "SKU-A",
                Amount = 29.99m,
                Currency = "USD",
                StockQuantity = 50,
                Size = "S",
                Color = "Red"
            },
            TestContext.Current.CancellationToken);

            await client.PostAsJsonAsync($"/api/products/{productId}/variants", new
            {
                Sku = "SKU-B",
                Amount = 39.99m,
                Currency = "USD",
                StockQuantity = 30,
                Size = "L",
                Color = "Green"
            },
            TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync(
                $"/api/products/{productId}/variants", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var variants = await response.Content
                .ReadFromJsonAsync<List<ProductVariantResponse>>(TestContext.Current.CancellationToken);

            variants.Should().NotBeNull();
            variants!.Count.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task GetProductVariantById_ShouldReturnVariant()
        {
            // Arrange
            var (client, productId) = await CreateClientWithProductAsync();

            var createResponse = await client.PostAsJsonAsync($"/api/products/{productId}/variants", new
            {
                Sku = "SKU-DETAIL",
                Amount = 59.99m,
                Currency = "EUR",
                StockQuantity = 10,
                Size = "XL",
                Color = "Black"
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content
                .ReadFromJsonAsync<CreatedVariantResponse>(TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync(
                $"/api/products/{productId}/variants/{created!.VariantId}",
                TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var variant = await response.Content
                .ReadFromJsonAsync<ProductVariantResponse>(TestContext.Current.CancellationToken);

            variant.Should().NotBeNull();
            variant!.Sku.Should().Be("SKU-DETAIL");
            variant.Amount.Should().Be(59.99m);
            variant.Currency.Should().Be("EUR");
            variant.Size.Should().Be("XL");
            variant.Color.Should().Be("Black");
            variant.StockQuantity.Should().Be(10);
        }

        [Fact]
        public async Task GetProductVariantById_ShouldReturnNotFound_WhenVariantDoesNotExist()
        {
            // Arrange
            var (client, productId) = await CreateClientWithProductAsync();

            // Act
            var response = await client.GetAsync(
                $"/api/products/{productId}/variants/{Guid.NewGuid()}",
                TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateProductVariant_ShouldReturnCreatedAtAction_WhenAdmin()
        {
            // Arrange
            var (client, productId) = await CreateClientWithProductAsync();

            var createResponse = await client.PostAsJsonAsync($"/api/products/{productId}/variants", new
            {
                Sku = "SKU-UPDATE",
                Amount = 19.99m,
                Currency = "USD",
                StockQuantity = 5,
                Size = "S",
                Color = "White"
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content
                .ReadFromJsonAsync<CreatedVariantResponse>(TestContext.Current.CancellationToken);

            // Act
            var updateResponse = await client.PutAsJsonAsync(
                $"/api/products/{productId}/variants/{created!.VariantId}", new
                {
                    Sku = "SKU-UPDATED",
                    Amount = 24.99m,
                    Currency = "EUR",
                    StockQuantity = 15,
                    Size = "M",
                    Color = "Gray"
                },
                TestContext.Current.CancellationToken);

            // Assert
            updateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var getResponse = await client.GetAsync(
                $"/api/products/{productId}/variants/{created.VariantId}",
                TestContext.Current.CancellationToken);

            var variant = await getResponse.Content
                .ReadFromJsonAsync<ProductVariantResponse>(TestContext.Current.CancellationToken);

            variant!.Sku.Should().Be("SKU-UPDATED");
            variant.Amount.Should().Be(24.99m);
            variant.Currency.Should().Be("EUR");
            variant.StockQuantity.Should().Be(15);
        }

        [Fact]
        public async Task DeleteProductVariant_ShouldReturnNoContent_WhenAdmin()
        {
            // Arrange
            var (client, productId) = await CreateClientWithProductAsync();

            var createResponse = await client.PostAsJsonAsync($"/api/products/{productId}/variants", new
            {
                Sku = "SKU-DELETE",
                Amount = 9.99m,
                Currency = "USD",
                StockQuantity = 1,
                Size = "L",
                Color = "Red"
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content
                .ReadFromJsonAsync<CreatedVariantResponse>(TestContext.Current.CancellationToken);

            // Act
            var deleteResponse = await client.DeleteAsync(
                $"/api/products/{productId}/variants/{created!.VariantId}",
                TestContext.Current.CancellationToken);

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await client.GetAsync(
                $"/api/products/{productId}/variants/{created.VariantId}",
                TestContext.Current.CancellationToken);

            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProductVariant_ShouldReturnForbidden_WhenCustomer()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.DeleteAsync(
                $"/api/products/{Guid.NewGuid()}/variants/{Guid.NewGuid()}",
                TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private static async Task<(HttpClient Client, Guid ProductId)> CreateClientWithProductAsync()
        {
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            var categoryResponse = await client.PostAsJsonAsync("/api/categories", new
            {
                Name = $"TestCategory-{Guid.NewGuid():N}",
                Description = "Test category"
            },
            TestContext.Current.CancellationToken);

            var createdCategory = await categoryResponse.Content
                .ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            var productResponse = await client.PostAsJsonAsync("/api/products", new
            {
                Name = $"TestProduct-{Guid.NewGuid():N}",
                Amount = 100.00m,
                Currency = "USD",
                CategoryId = createdCategory!.Id,
                Description = "Test product"
            },
            TestContext.Current.CancellationToken);

            var createdProduct = await productResponse.Content
                .ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            return (client, createdProduct!.Id);
        }

        private record CreatedResponse(Guid Id);
        private record CreatedVariantResponse(Guid ProductId, Guid VariantId);
    }
}
