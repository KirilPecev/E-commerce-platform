using CatalogService.Domain.Events;

namespace CatalogService.Application
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IDomainEvent domainEvent);
    }
}
