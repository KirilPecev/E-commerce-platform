
using CatalogService.Application.Interfaces;

namespace CatalogService.Infrastructure.Messaging
{
    public class MassTransitEventPublisher
        (IEventPublisher eventPublisher) : IEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
        {
            return eventPublisher.PublishAsync(@event);
        }
    }
}
