using CatalogService.Application.Categories.Commands;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests.ApplicationTests
{
    public class DeleteCategoryCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldDeleteCategory_WhenCategoryExists()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new DeleteCategoryCommandHandler(dbContext);

            var categoryId = Guid.Parse("11111111-0000-0000-0000-000000000001");
            var command = new DeleteCategoryCommand(categoryId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var deleted = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId, TestContext.Current.CancellationToken);

            deleted.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenCategoryNotFound()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync();

            var handler = new DeleteCategoryCommandHandler(dbContext);

            var command = new DeleteCategoryCommand(Guid.NewGuid());

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
