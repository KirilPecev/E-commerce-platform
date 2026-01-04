using MediatR;

namespace InventoryService.Application.Inventory.Commands
{
    public record ReleaseStockCommand(
        Guid ProductId,
        Guid ProductVariantId,
        Guid OrderId) : IRequest;
}
