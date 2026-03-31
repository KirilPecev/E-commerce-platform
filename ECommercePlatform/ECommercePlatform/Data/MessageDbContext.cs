using System.Reflection;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data.Configuration;
using ECommercePlatform.Data.Models;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Events;

using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Data
{
    public abstract class MessageDbContext : DbContext
    {
        private readonly IDomainEventDispatcher dispatcher;

        protected MessageDbContext(
            DbContextOptions options,
            IDomainEventDispatcher dispatcher)
            : base(options)
        {
            this.dispatcher = dispatcher;
        }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected abstract Assembly ConfigurationsAssembly { get; }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await DispatchDomainEventsAsync();

            int result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new OutboxMessageConfiguration());

            builder.ApplyConfigurationsFromAssembly(this.ConfigurationsAssembly);

            base.OnModelCreating(builder);
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
