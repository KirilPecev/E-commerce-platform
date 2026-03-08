using CatalogService.Application.Categories.Commands;
using CatalogService.Domain.Aggregates;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests.ApplicationTests
{
    public class UpdateCategoryCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldUpdateCategory_WhenCategoryExists()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new UpdateCategoryCommandHandler(dbContext);

            var categoryId = Guid.Parse("11111111-0000-0000-0000-000000000001");
            var command = new UpdateCategoryCommand(categoryId, "Updated Electronics", "Updated description");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(categoryId);

            var updated = await dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == categoryId, TestContext.Current.CancellationToken);

            updated.Should().NotBeNull();
            updated!.Name.Should().Be("Updated Electronics");
            updated.Description.Should().Be("Updated description");
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenCategoryNotFound()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync();

            var handler = new UpdateCategoryCommandHandler(dbContext);

            var command = new UpdateCategoryCommand(Guid.NewGuid(), "Name", "Desc");

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
