using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;

namespace CatalogService.Tests.IntegrationTests
{
    public class ExceptionHandlingMiddlewareTests : IClassFixture<CatalogWebApplicationFactory>
    {
        private readonly CatalogWebApplicationFactory _factory;

        public ExceptionHandlingMiddlewareTests(CatalogWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateProduct_WithMissingCategory_ReturnsNotFound_AndIncludesCorrelationId()
        {
            var client = _factory
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"))
                .CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            var payload = new
            {
                Name = "Test Product",
                Amount = 9.99m,
                Currency = "USD",
                CategoryId = Guid.NewGuid(), // not seeded -> will cause KeyNotFoundException
                Description = ""
            };

            var response = await client.PostAsJsonAsync("/api/products", payload, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
            response.Headers.Contains("X-Correlation-Id").Should().BeTrue();

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);

            doc.RootElement.GetProperty("status").GetInt32().Should().Be((int)HttpStatusCode.NotFound);
            doc.RootElement.GetProperty("title").GetString().Should().Be("Resource not found");
            doc.RootElement.TryGetProperty("correlationId", out var corr).Should().BeTrue();
            corr.GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateProduct_WithInvalidName_ThrowsDomainException_ReturnsBadRequest_AndIncludesCorrelationId()
        {
            var client = _factory
                .WithWebHostBuilder(b => b.UseEnvironment("Testing"))
                .CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", CatalogTestTokenGenerator.GenerateAdminToken());

            var payload = new
            {
                Name = "", // invalid -> ProductName throws CatalogDomainException
                Amount = 9.99m,
                Currency = "USD",
                CategoryId = Guid.Parse("11111111-0000-0000-0000-000000000001"), // seeded category
                Description = ""
            };

            var response = await client.PostAsJsonAsync("/api/products", payload, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response!.Content!.Headers!.ContentType!.MediaType.Should().Be("application/problem+json");
            response.Headers.Contains("X-Correlation-Id").Should().BeTrue();

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);

            doc.RootElement.GetProperty("status").GetInt32().Should().Be((int)HttpStatusCode.BadRequest);
            doc.RootElement.GetProperty("title").GetString().Should().Be("Domain error");
            doc.RootElement.TryGetProperty("correlationId", out var corr).Should().BeTrue();
            corr.GetString().Should().NotBeNullOrEmpty();
        }
    }
}
