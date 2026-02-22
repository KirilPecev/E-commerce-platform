using CatalogService.Application.Interfaces;
using CatalogService.Application.Products.Queries;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CatalogService.Tests.ApplicationTests
{
    public class GetAllProductsQueryHandlerTests
    {
        private readonly Mock<IProductCache> cacheMock;

        public GetAllProductsQueryHandlerTests()
        {
            cacheMock = new Mock<IProductCache>();
        }

        [Fact]
        public async Task Handle_ShouldReturnProductsFromCache_WhenCacheExists()
        {
            var cachedProducts = new List<ProductDto>
            {
                new(Guid.NewGuid(), "Laptop", 1500, "USD",  Guid.Parse("11111111-0000-0000-0000-000000000001"), "Gaming Laptop")
            };

            cacheMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(cachedProducts);

            var handler = new GetAllProductsQueryHandler(
                dbContext: null!,
                cacheMock.Object
            );

            var result = await handler.Handle(new GetAllProductsQuery(), TestContext.Current.CancellationToken);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ShouldLoadFromDb_AndCacheResult_WhenCacheMiss()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            Category? category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == Guid.Parse("11111111-0000-0000-0000-000000000002"), TestContext.Current.CancellationToken);

            // Act
            Product product = new Product(
                new ProductName("Laptop"),
                new Money(1500m, "USD"),
                category!,
                "High-end gaming laptop"
            );

            await dbContext.Products.AddAsync(product, TestContext.Current.CancellationToken);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            cacheMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync((IReadOnlyList<ProductDto>?)null);

            var handler = new GetAllProductsQueryHandler(dbContext, cacheMock.Object);

            var result = await handler.Handle(new GetAllProductsQuery(), TestContext.Current.CancellationToken);

            result.Should().NotBeEmpty();

            cacheMock.Verify(x => x.SetAllAsync(It.IsAny<IReadOnlyList<ProductDto>>()),
                Times.Once);
        }
    }
}
