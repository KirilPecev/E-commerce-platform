using ECommercePlatform.Application.Interfaces;

using InventoryService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace InventoryService.Tests
{
    public static class InventoryDbContextFactory
    {
        public static async Task<InventoryDbContext> CreateAsync()
        {
            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new InventoryDbContext(options, dispatcherMock.Object);

            return context;
        }
    }
}
