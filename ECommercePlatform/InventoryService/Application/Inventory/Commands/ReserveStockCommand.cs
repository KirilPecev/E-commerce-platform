using MediatR;

namespace InventoryService.Application.Inventory.Commands
{
    public record ReserveStockCommand(
        Guid OrderId,
        Guid ProductId,
        Guid ProductVariantId,
        int Quantity
        ) : IRequest;
}
