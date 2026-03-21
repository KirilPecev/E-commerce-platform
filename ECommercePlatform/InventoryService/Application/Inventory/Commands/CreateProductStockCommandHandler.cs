
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Aggregates;

using MediatR;

namespace InventoryService.Application.Inventory.Commands
{
    public class CreateProductStockCommandHandler
        (IInventoryDbContext inventoryDbContext) : IRequestHandler<CreateProductStockCommand, Guid>
    {
        public async Task<Guid> Handle(CreateProductStockCommand request, CancellationToken cancellationToken)
        {
            ProductStock productStock = new ProductStock(request.ProductId, request.ProductVariantId, request.InitialQuantity);

            inventoryDbContext.ProductStocks.Add(productStock);

            await inventoryDbContext.SaveChangesAsync(cancellationToken);

            return productStock.Id;
        }
    }
}
