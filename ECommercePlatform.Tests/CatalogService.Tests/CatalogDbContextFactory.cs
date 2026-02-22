using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Persistence.Seeding;

using ECommercePlatform.Application.Interfaces;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace CatalogService.Tests
{
    public static class CatalogDbContextFactory
    {
        public static async Task<CatalogDbContext> CreateAsync(bool seedCategories = false)
        {
            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new CatalogDbContext(options, dispatcherMock.Object);

            // Ensure DB created (no-op for InMemory but explicit is clearer)
            await context.Database.EnsureCreatedAsync();

            if (seedCategories)
                await CategoriesSeeder.SeedCategoriesAsync(context);

            return context;
        }
    }
}
