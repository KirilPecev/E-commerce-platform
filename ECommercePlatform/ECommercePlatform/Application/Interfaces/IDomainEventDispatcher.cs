using ECommercePlatform.Domain.Events;

namespace ECommercePlatform.Application.Interfaces
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IDomainEvent domainEvent);
    }
}
