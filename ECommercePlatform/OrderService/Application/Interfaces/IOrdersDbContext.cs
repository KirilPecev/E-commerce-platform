using Microsoft.EntityFrameworkCore;

using OrderService.Domain.Aggregates;

namespace OrderService.Application.Interfaces
{
    public interface IOrdersDbContext
    {
        DbSet<Order> Orders { get; }
        DbSet<OrderItem> Items { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
