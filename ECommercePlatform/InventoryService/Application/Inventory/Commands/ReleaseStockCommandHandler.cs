using InventoryService.Application.Interfaces;
using InventoryService.Domain.Aggregates;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Inventory.Commands
{
    public class ReleaseStockCommandHandler
        (IInventoryDbContext inventoryDbContext) : IRequestHandler<ReleaseStockCommand>
    {
        public async Task Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
        {
            ProductStock? productStock = await inventoryDbContext
                .ProductStocks
                .Include(ps => ps.Reservations)
                .FirstOrDefaultAsync(ps => ps.ProductId == request.ProductId
                                        && ps.ProductVariantId == request.ProductVariantId, cancellationToken);

            if (productStock == null)
            {
                throw new InvalidOperationException($"Stock not found.");
            }

            productStock.Release(request.OrderId);

            await inventoryDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
