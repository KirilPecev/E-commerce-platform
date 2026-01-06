using MediatR;

namespace InventoryService.Application.Inventory.Commands
{
    public record ReleaseStocksForOrderCommand(
        Guid OrderId,
        string Reason
        ) : IRequest;
}
