using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using CatalogService.Contracts.Responses;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;

namespace CatalogService.Tests.IntegrationTests
{
    public class CategoriesTests : IClassFixture<CatalogWebApplicationFactory>
    {
        [Fact]
        public async Task Create_ShouldReturnCreated_WhenAdmin()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            // Act
            var response = await client.PostAsJsonAsync("/api/categories", new
            {
                Name = "Test Category",
                Description = "A test category"
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
            var response = await client.PostAsJsonAsync("/api/categories", new
            {
                Name = "Test Category",
                Description = "A test category"
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
            var response = await client.PostAsJsonAsync("/api/categories", new
            {
                Name = "Test Category",
                Description = "A test category"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_ShouldThrow_WhenDuplicateName()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            await client.PostAsJsonAsync("/api/categories", new
            {
                Name = "Duplicate",
                Description = "First"
            },
            TestContext.Current.CancellationToken);

            // Act
            var response = await client.PostAsJsonAsync("/api/categories", new
            {
                Name = "Duplicate",
                Description = "Second"
            },
            TestContext.Current.CancellationToken);

            // Assert: middleware converts exceptions to ProblemDetails responses
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response!.Content!.Headers!.ContentType!.MediaType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task GetById_ShouldReturnCategory()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            var createResponse = await client.PostAsJsonAsync("/api/categories", new
            {
                Name = "Test Electronics",
                Description = "Test Electronic devices"
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync(
                $"/api/categories/{created!.Id}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var category = await response.Content.ReadFromJsonAsync<CategoryResponse>(TestContext.Current.CancellationToken);

            category.Should().NotBeNull();
            category!.Name.Should().Be("Test Electronics");
            category.Description.Should().Be("Test Electronic devices");
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync(
                $"/api/categories/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAll_ShouldReturnCategories()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            await client.PostAsJsonAsync("/api/categories", new
            {
                Name = "Category A",
                Description = "First"
            },
            TestContext.Current.CancellationToken);

            await client.PostAsJsonAsync("/api/categories", new
            {
                Name = "Category B",
                Description = "Second"
            },
            TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync("/api/categories", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var categories = await response.Content
                .ReadFromJsonAsync<List<CategoryResponse>>(TestContext.Current.CancellationToken);

            categories.Should().NotBeNull();
            categories!.Count.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task Update_ShouldReturnCreatedAtAction_WhenAdmin()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            var createResponse = await client.PostAsJsonAsync("/api/categories", new
            {
                Name = "Old Name",
                Description = "Old description"
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            // Act
            var updateResponse = await client.PutAsJsonAsync($"/api/categories/{created!.Id}", new
            {
                Name = "Updated Name",
                Description = "Updated description"
            },
            TestContext.Current.CancellationToken);

            // Assert
            updateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var getResponse = await client.GetAsync(
                $"/api/categories/{created.Id}", TestContext.Current.CancellationToken);

            var category = await getResponse.Content.ReadFromJsonAsync<CategoryResponse>(TestContext.Current.CancellationToken);

            category!.Name.Should().Be("Updated Name");
            category.Description.Should().Be("Updated description");
        }

        [Fact]
        public async Task Update_ShouldReturnForbidden_WhenCustomer()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.PutAsJsonAsync($"/api/categories/{Guid.NewGuid()}", new
            {
                Name = "Name",
                Description = "Description"
            },
            TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenAdmin()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            var createResponse = await client.PostAsJsonAsync("/api/categories", new
            {
                Name = "To Delete",
                Description = "Will be deleted"
            },
            TestContext.Current.CancellationToken);

            var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>(TestContext.Current.CancellationToken);

            // Act
            var deleteResponse = await client.DeleteAsync(
                $"/api/categories/{created!.Id}", TestContext.Current.CancellationToken);

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await client.GetAsync(
                $"/api/categories/{created.Id}", TestContext.Current.CancellationToken);

            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ShouldReturnForbidden_WhenCustomer()
        {
            // Arrange
            var factory = new CatalogWebApplicationFactory()
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"));

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateCustomerToken());

            // Act
            var response = await client.DeleteAsync(
                $"/api/categories/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        private record CreatedResponse(Guid Id);
    }
}
