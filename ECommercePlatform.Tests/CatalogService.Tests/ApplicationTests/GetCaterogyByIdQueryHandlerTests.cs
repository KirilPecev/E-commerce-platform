using CatalogService.Application.Categories.Queries;

using FluentAssertions;

namespace CatalogService.Tests.ApplicationTests
{
    public class GetCaterogyByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnCategory_WhenCategoryExists()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new GetCaterogyByIdQueryHandler(dbContext);

            var categoryId = Guid.Parse("11111111-0000-0000-0000-000000000009");
            var query = new GetCaterogyByIdQuery(categoryId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(categoryId);
            result.Name.Should().Be("Automotive");
            result.Description.Should().Be("Auto parts, tools and vehicle accessories.");
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenCategoryNotFound()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync();

            var handler = new GetCaterogyByIdQueryHandler(dbContext);

            var query = new GetCaterogyByIdQuery(Guid.NewGuid());

            // Act
            Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
