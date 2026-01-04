using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Events;

using InventoryService.Domain.Aggregates;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Persistence
{
    public class InventoryDbContext : DbContext
    {
        public readonly IDomainEventDispatcher dispatcher;

        public InventoryDbContext(DbContextOptions<InventoryDbContext> options, IDomainEventDispatcher dispatcher)
            : base(options)
        {
            this.dispatcher = dispatcher;
        }

        public DbSet<ProductStock> ProductStock { get; set; }
        public DbSet<StockReservation> StockReservations { get; set; }

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
                typeof(InventoryDbContext).Assembly);
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
