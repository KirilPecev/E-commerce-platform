using ECommercePlatform.Application.Interfaces;

using FluentAssertions;

using InventoryService.Application.Inventory.Queries;
using InventoryService.Domain.Aggregates;
using InventoryService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace InventoryService.Tests.ApplicationTests
{
    public class GetProductStocksQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoStocks()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new InventoryDbContext(options, dispatcherMock.Object);

            var handler = new GetProductStocksQueryHandler(context);

            var result = await handler.Handle(new GetProductStocksQuery(Guid.NewGuid()), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsProductStocks_WithCorrectQuantities()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new InventoryDbContext(options, dispatcherMock.Object))
            {
                var productId = Guid.NewGuid();

                var variant1 = Guid.NewGuid();
                var stock1 = new ProductStock(productId, variant1, 10);
                var orderA = Guid.NewGuid();
                stock1.Reserve(orderA, 3); // available 7, reserved 3

                var variant2 = Guid.NewGuid();
                var stock2 = new ProductStock(productId, variant2, 5);
                var orderB = Guid.NewGuid();
                stock2.Reserve(orderB, 2);
                stock2.Confirm(orderB); // available 3, reserved 2 (confirmed)

                // different product
                var otherStock = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 8);
                otherStock.Reserve(Guid.NewGuid(), 1);

                context.ProductStocks.AddRange(stock1, stock2, otherStock);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                // reset mock to ignore initial domain event dispatches
                dispatcherMock.Reset();

                var handler = new GetProductStocksQueryHandler(context);

                var result = await handler.Handle(new GetProductStocksQuery(productId), CancellationToken.None);

                result.Should().HaveCount(2);

                result.Should().ContainSingle(r => r.ProductVariantId == variant1 && r.QuantityAvailable == 7 && r.QuantityReserved == 3);
                result.Should().ContainSingle(r => r.ProductVariantId == variant2 && r.QuantityAvailable == 3 && r.QuantityReserved == 2);
            }
        }
    }
}
