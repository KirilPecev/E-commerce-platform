
using InventoryService.Domain.Aggregates;
using InventoryService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Inventory.Commands
{
    public class ReserveStockCommandHandler
        (InventoryDbContext inventoryDbContext) : IRequestHandler<ReserveStockCommand>
    {
        public async Task Handle(ReserveStockCommand request, CancellationToken cancellationToken)
        {
            ProductStock? productStock = await inventoryDbContext
                .ProductStocks
                .FirstOrDefaultAsync(ps => ps.ProductId == request.ProductId
                                        && ps.ProductVariantId == request.ProductVariantId, cancellationToken);

            if (productStock == null)
            {
                throw new InvalidOperationException($"Stock not found.");
            }

            productStock.Reserve(request.OrderId, request.Quantity);

            await inventoryDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
