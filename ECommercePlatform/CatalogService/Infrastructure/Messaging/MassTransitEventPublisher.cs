
using CatalogService.Application.Interfaces;

using MassTransit;

namespace CatalogService.Infrastructure.Messaging
{
    public class MassTransitEventPublisher
        (IPublishEndpoint publishEndpoint) : IEventPublisher
    {
        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
        {
            await publishEndpoint.Publish(@event);
        }
    }
}
