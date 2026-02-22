using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using InventoryService.Application.Inventory.Commands;
using InventoryService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace InventoryService.Tests.ApplicationTests
{
    public class CreateProductStockCommandHandlerTests
    {
        [Fact]
        public async Task Handle_CreatesProductStockAndPersists()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new InventoryDbContext(options, dispatcherMock.Object);

            var handler = new CreateProductStockCommandHandler(context);

            var cmd = new CreateProductStockCommand(Guid.NewGuid(), Guid.NewGuid(), 7);

            var resultId = await handler.Handle(cmd, CancellationToken.None);

            resultId.Should().NotBe(Guid.Empty);

            var saved = await context.ProductStocks.FindAsync(new object[] { resultId }, CancellationToken.None);
            saved.Should().NotBeNull();
            saved!.ProductId.Should().Be(cmd.ProductId);
            saved.ProductVariantId.Should().Be(cmd.ProductVariantId);
            saved.AvailableQuantity.Should().Be(7);

            // No domain events were added, so dispatcher should not be called
            dispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IDomainEvent>()), Times.Never);
        }
    }
}
