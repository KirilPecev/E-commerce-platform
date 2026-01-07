using MediatR;

namespace InventoryService.Application.Inventory.Queries
{
    public record GetProductStocksQuery(Guid ProductId) : IRequest<List<ProductStockDto>>;
}
