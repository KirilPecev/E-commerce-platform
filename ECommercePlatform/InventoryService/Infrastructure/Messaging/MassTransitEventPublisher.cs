using ECommercePlatform.Application.Interfaces;

using MassTransit;

namespace InventoryService.Infrastructure.Messaging
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
