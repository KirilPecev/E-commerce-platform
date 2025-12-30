using Microsoft.EntityFrameworkCore;

using OrderService.Domain.Aggregates;

namespace OrderService.Infrastructure.Persistence
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderItem> Items { get; set; } = default!;
    }
}
