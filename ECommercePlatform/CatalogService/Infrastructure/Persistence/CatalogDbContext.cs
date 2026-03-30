using System.Reflection;

using CatalogService.Application.Interfaces;
using CatalogService.Domain.Aggregates;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Events;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Persistence
{
    public class CatalogDbContext : MessageDbContext, ICatalogDbContext
    {
        public readonly IDomainEventDispatcher dispatcher;

        public CatalogDbContext(
            DbContextOptions<CatalogDbContext> options,
            IDomainEventDispatcher dispatcher) : base(options)
        {
            this.dispatcher = dispatcher;
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }

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