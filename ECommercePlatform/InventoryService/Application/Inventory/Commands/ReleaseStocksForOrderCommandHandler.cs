
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Aggregates;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Inventory.Commands
{
    public class ReleaseStocksForOrderCommandHandler
        (IInventoryDbContext inventoryDbContext) : IRequestHandler<ReleaseStocksForOrderCommand>
    {
        public async Task Handle(ReleaseStocksForOrderCommand request, CancellationToken cancellationToken)
        {
            List<ProductStock> productStocks = await inventoryDbContext
                 .ProductStocks
                 .Include(ps => ps.Reservations)
                 .AsAsyncEnumerable()
                 .Where(ps => ps.HasReservedStockForOrder(request.OrderId))
                 .ToListAsync();

            foreach (ProductStock productStock in productStocks)
            {
                productStock.Release(request.OrderId);
            }

            await inventoryDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
