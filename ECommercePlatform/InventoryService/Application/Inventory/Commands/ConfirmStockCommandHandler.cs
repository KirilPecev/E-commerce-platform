
using InventoryService.Domain.Aggregates;
using InventoryService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Inventory.Commands
{
    public class ConfirmStockCommandHandler
        (InventoryDbContext inventoryDbContext) : IRequestHandler<ConfirmStockCommand>
    {
        public async Task Handle(ConfirmStockCommand request, CancellationToken cancellationToken)
        {
            List<ProductStock> productStocks = await inventoryDbContext
                 .ProductStocks
                 .Include(ps => ps.Reservations)
                 .AsAsyncEnumerable()
                 .Where(ps => ps.HasReservedPendingStockForOrder(request.OrderId))
                 .ToListAsync();

            foreach (ProductStock productStock in productStocks)
            {
                productStock.Confirm(request.OrderId);
            }

            await inventoryDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
