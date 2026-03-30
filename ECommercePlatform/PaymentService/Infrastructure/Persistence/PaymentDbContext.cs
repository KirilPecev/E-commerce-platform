using System.Reflection;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;
using ECommercePlatform.Domain.Abstractions;
using ECommercePlatform.Domain.Events;

using Microsoft.EntityFrameworkCore;

using PaymentService.Application.Interfaces;
using PaymentService.Domain.Aggregates;

namespace PaymentService.Infrastructure.Persistence
{
    public class PaymentDbContext : MessageDbContext, IPaymentDbContext
    {
        public readonly IDomainEventDispatcher dispatcher;

        public PaymentDbContext(DbContextOptions<PaymentDbContext> options, IDomainEventDispatcher dispatcher)
            : base(options)
        {
            this.dispatcher = dispatcher;
        }

        public DbSet<Payment> Payments { get; set; } = default!;

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
