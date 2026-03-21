using CatalogService.Application.Categories.Queries;

using FluentAssertions;

namespace CatalogService.Tests.ApplicationTests
{
    public class GetAllCategoriesQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnAllCategories_WhenCategoriesExist()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new GetAllCategoriesQueryHandler(dbContext);

            // Act
            var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

            // Assert
            var categories = result.ToList();
            categories.Should().NotBeEmpty();
            categories.Should().HaveCount(14); // 14 seeded categories
            categories.Should().AllSatisfy(c =>
            {
                c.Id.Should().NotBeEmpty();
                c.Name.Should().NotBeNullOrEmpty();
            });
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoCategoriesExist()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync();

            var handler = new GetAllCategoriesQueryHandler(dbContext);

            // Act
            var result = await handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
