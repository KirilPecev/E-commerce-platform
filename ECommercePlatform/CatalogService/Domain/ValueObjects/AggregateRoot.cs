using CatalogService.Domain.Common;
using CatalogService.Domain.Events;

namespace CatalogService.Domain.ValueObjects
{
    public abstract class AggregateRoot : Entity
    {
        private readonly List<IDomainEvent> domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents;

        protected void AddDomainEvent(IDomainEvent @event)
            => domainEvents.Add(@event);

        public void ClearDomainEvents()
            => domainEvents.Clear();
    }
}
