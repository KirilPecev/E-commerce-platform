using System.Reflection;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;

using Microsoft.EntityFrameworkCore;

using OrderService.Application.Interfaces;
using OrderService.Domain.Aggregates;

namespace OrderService.Infrastructure.Persistence
{
    public class OrdersDbContext : MessageDbContext, IOrdersDbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options, IDomainEventDispatcher dispatcher)
            : base(options, dispatcher)
        {
        }

        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderItem> Items { get; set; } = default!;

        protected override Assembly ConfigurationsAssembly => Assembly.GetExecutingAssembly();
    }
}
