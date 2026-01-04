using MediatR;

namespace InventoryService.Application.Inventory.Commands
{
    public record ConfirmStockCommand(
        Guid OrderId,
        Guid ProductId,
        Guid ProductVariantId
        ) : IRequest;
}
