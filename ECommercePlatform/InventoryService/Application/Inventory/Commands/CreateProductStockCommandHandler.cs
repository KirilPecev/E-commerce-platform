
using InventoryService.Domain.Aggregates;
using InventoryService.Infrastructure.Persistence;

using MediatR;

namespace InventoryService.Application.Inventory.Commands
{
    public class CreateProductStockCommandHandler
        (InventoryDbContext inventoryDbContext) : IRequestHandler<CreateProductStockCommand, Guid>
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
