using MediatR;

namespace InventoryService.Application.Inventory.Commands
{
    public record CreateProductStockCommand(
        Guid ProductId,
        Guid ProductVariantId,
        int InitialQuantity) : IRequest<Guid>;
}
