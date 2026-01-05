
using InventoryService.Domain.Aggregates;
using InventoryService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Inventory.Commands
{
    public class UpdateProductStockCommandHandler
        (InventoryDbContext inventoryDbContext) : IRequestHandler<UpdateProductStockCommand>
    {
        public async Task Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
        {
            ProductStock? productStock = await inventoryDbContext
                .ProductStocks
                .FirstOrDefaultAsync(ps => ps.ProductId == request.ProductId
                                        && ps.ProductVariantId == request.ProductVariantId,
                                        cancellationToken);

            if (productStock == null)
                throw new InvalidOperationException("Product stock not found.");

            productStock.UpdateQuantity(request.Quantity);

            await inventoryDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
