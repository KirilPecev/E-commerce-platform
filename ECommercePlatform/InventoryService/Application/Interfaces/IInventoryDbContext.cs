using InventoryService.Domain.Aggregates;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Interfaces
{
    public interface IInventoryDbContext
    {
        DbSet<ProductStock> ProductStocks { get; }
        DbSet<StockReservation> StockReservations { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
