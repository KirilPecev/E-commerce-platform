using System.Reflection;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Events;

using InventoryService.Application.Interfaces;
using InventoryService.Domain.Aggregates;

using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Persistence
{
    public class InventoryDbContext : MessageDbContext, IInventoryDbContext
    {
        public readonly IDomainEventDispatcher dispatcher;

        public InventoryDbContext(DbContextOptions<InventoryDbContext> options, IDomainEventDispatcher dispatcher)
            : base(options)
        {
            this.dispatcher = dispatcher;
        }

        public DbSet<ProductStock> ProductStocks { get; set; }
        public DbSet<StockReservation> StockReservations { get; set; }

        protected override Assembly ConfigurationsAssembly => Assembly.GetExecutingAssembly();

        public override async Task<int> SaveChangesAsync(
           CancellationToken cancellationToken = default)
        {
            await DispatchDomainEventsAsync();

            int result = await base.SaveChangesAsync(cancellationToken);

            return result;
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
