using MediatR;

namespace InventoryService.Application.Inventory.Commands
{
    public record UpdateProductStockCommand(
        Guid ProductId,
        Guid ProductVariantId,
        int Quantity) : IRequest;
}
