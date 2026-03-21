using CatalogService.Application.Categories.Commands;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests.ApplicationTests
{
    public class CreateCategoryCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldCreateCategory_WhenNameIsUnique()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync();

            var handler = new CreateCategoryCommandHandler(dbContext);

            var command = new CreateCategoryCommand("New Category", "A test category");

            // Act
            var id = await handler.Handle(command, CancellationToken.None);

            // Assert
            id.Should().NotBeEmpty();

            var category = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);

            category.Should().NotBeNull();
            category!.Name.Should().Be("New Category");
            category.Description.Should().Be("A test category");
        }

        [Fact]
        public async Task Handle_ShouldCreateCategory_WithNullDescription()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync();

            var handler = new CreateCategoryCommandHandler(dbContext);

            var command = new CreateCategoryCommand("Minimal Category", null);

            // Act
            var id = await handler.Handle(command, CancellationToken.None);

            // Assert
            id.Should().NotBeEmpty();

            var category = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == id, TestContext.Current.CancellationToken);

            category.Should().NotBeNull();
            category!.Name.Should().Be("Minimal Category");
            category.Description.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenCategoryNameAlreadyExists()
        {
            var dbContext = await CatalogDbContextFactory.CreateAsync(seedCategories: true);

            var handler = new CreateCategoryCommandHandler(dbContext);

            // "Electronics" is one of the seeded categories
            var command = new CreateCategoryCommand("Automotive", "Duplicate");

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }
    }
}
