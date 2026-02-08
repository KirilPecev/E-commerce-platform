
using InventoryService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Inventory.Queries
{
    public class GetProductStocksQueryHandler
        (InventoryDbContext inventoryDb) : IRequestHandler<GetProductStocksQuery, List<ProductStockDto>>
    {
        public async Task<List<ProductStockDto>> Handle(GetProductStocksQuery request, CancellationToken cancellationToken)
            => await inventoryDb
                .ProductStocks
                .Include(ps => ps.Reservations)
                .Where(ps => ps.ProductId == request.ProductId)
                .Select(ps => new ProductStockDto(ps.ProductId, ps.ProductVariantId, ps.AvailableQuantity, ps.ReservedQuantity))
                .ToListAsync();
    }
}
