using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Events;

using Microsoft.EntityFrameworkCore;

using OrderService.Domain.Aggregates;

namespace OrderService.Infrastructure.Persistence
{
    public class OrdersDbContext : DbContext
    {
        public readonly IDomainEventDispatcher dispatcher;

        public OrdersDbContext(DbContextOptions<OrdersDbContext> options, IDomainEventDispatcher dispatcher)
            : base(options)
        {
            this.dispatcher = dispatcher;
        }

        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderItem> Items { get; set; } = default!;

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            int result = await base.SaveChangesAsync(cancellationToken);

            await DispatchDomainEventsAsync();

            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(OrdersDbContext).Assembly);
        }

        private async Task DispatchDomainEventsAsync()
        {
            List<AggregateRoot> aggregates = ChangeTracker
                .Entries<AggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            List<IDomainEvent> domainEvents = aggregates
                .SelectMany(a => a.DomainEvents)
                .ToList();

            aggregates.ForEach(a => a.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await dispatcher.DispatchAsync(domainEvent);
            }
        }
    }
}
