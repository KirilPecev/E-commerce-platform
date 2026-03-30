using System.Reflection;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Events;

using Microsoft.EntityFrameworkCore;

using OrderService.Application.Interfaces;
using OrderService.Domain.Aggregates;

namespace OrderService.Infrastructure.Persistence
{
    public class OrdersDbContext : MessageDbContext, IOrdersDbContext
    {
        public readonly IDomainEventDispatcher dispatcher;

        public OrdersDbContext(DbContextOptions<OrdersDbContext> options, IDomainEventDispatcher dispatcher)
            : base(options)
        {
            this.dispatcher = dispatcher;
        }

        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderItem> Items { get; set; } = default!;

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
