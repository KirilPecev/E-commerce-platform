using CatalogService.Application.Interfaces;
using CatalogService.Application.Products.Queries;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace CatalogService.Tests.ApplicationTests
{
    public class GetProductByIdQueryHandlerTests
    {
        private readonly Mock<IProductCache> cacheMock;

        public GetProductByIdQueryHandlerTests()
        {
            cacheMock = new Mock<IProductCache>();
        }

        [Fact]
        public async Task Handle_ShouldReturnProductsFromCache_WhenCacheExists()
        {
            var guid = Guid.NewGuid();
            var cachedProuct = new ProductDto(guid, "Laptop", 1500, "USD",  Guid.Parse("11111111-0000-0000-0000-000000000001"), "Gaming Laptop");

            cacheMock.Setup(x => x.GetByIdAsync(guid))
                .ReturnsAsync(cachedProuct);

            var handler = new GetProductByIdQueryHandler(
                dbContext: null!,
                cacheMock.Object
            );

            var result = await handler.Handle(new GetProductByIdQuery(guid), TestContext.Current.CancellationToken);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ShouldLoadFromDb_AndCacheResult_WhenCacheMiss()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            Category? category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000001"), TestContext.Current.CancellationToken);

            // Act
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                category!,
                "High-end gaming laptop"
            );

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            cacheMock.Setup(x => x.GetByIdAsync(product.Id))
                .ReturnsAsync((ProductDto?)null);

            var handler = new GetProductByIdQueryHandler(dbContext, cacheMock.Object);

            var result = await handler.Handle(new GetProductByIdQuery(product.Id), TestContext.Current.CancellationToken);

            result.Should().NotBeNull();

            cacheMock.Verify(x => x.SetByIdAsync(It.IsAny<ProductDto>()), Times.Once);
        }
    }
}
