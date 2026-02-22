using ECommercePlatform.Application.Interfaces;

using Microsoft.EntityFrameworkCore;

using Moq;

using OrderService.Infrastructure.Persistence;

namespace OrderService.Tests
{
    public static class InventoryDbContextFactory
    {
        public static async Task<OrdersDbContext> CreateAsync()
        {
            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new OrdersDbContext(options, dispatcherMock.Object);

            return context;
        }
    }
}
