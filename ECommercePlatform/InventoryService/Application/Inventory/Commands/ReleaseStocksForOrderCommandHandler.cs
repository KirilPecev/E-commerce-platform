
using InventoryService.Domain.Aggregates;
using InventoryService.Infrastructure.Persistence;

using MediatR;

namespace InventoryService.Application.Inventory.Commands
{
    public class ReleaseStocksForOrderCommandHandler
        (InventoryDbContext inventoryDbContext) : IRequestHandler<ReleaseStocksForOrderCommand>
    {
        public async Task Handle(ReleaseStocksForOrderCommand request, CancellationToken cancellationToken)
        {
            List<ProductStock> productStocks = inventoryDbContext
                 .ProductStocks
                 .Where(ps => ps.HasReservedStockForOrder(request.OrderId))
                 .ToList();

            foreach (ProductStock productStock in productStocks)
            {
                productStock.Release(request.OrderId);
            }

            await inventoryDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
