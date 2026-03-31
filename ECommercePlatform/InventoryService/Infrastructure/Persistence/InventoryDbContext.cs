using System.Reflection;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;

using InventoryService.Application.Interfaces;
using InventoryService.Domain.Aggregates;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Persistence
{
    public class InventoryDbContext : MessageDbContext, IInventoryDbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options, IDomainEventDispatcher dispatcher)
            : base(options, dispatcher)
        {
        }

        public DbSet<ProductStock> ProductStocks { get; set; }
        public DbSet<StockReservation> StockReservations { get; set; }

        protected override Assembly ConfigurationsAssembly => Assembly.GetExecutingAssembly();
    }
}
