using ECommercePlatform.Application.Interfaces;

using Microsoft.EntityFrameworkCore;

using Moq;

using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Tests
{
    public static class InventoryDbContextFactory
    {
        public static async Task<PaymentDbContext> CreateAsync()
        {
            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new PaymentDbContext(options, dispatcherMock.Object);

            return context;
        }
    }
}
