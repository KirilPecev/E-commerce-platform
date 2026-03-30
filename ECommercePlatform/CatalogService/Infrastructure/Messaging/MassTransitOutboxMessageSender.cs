using ECommercePlatform.Application.Interfaces;

using MassTransit;

namespace CatalogService.Infrastructure.Messaging
{
    public class MassTransitOutboxMessageSender
        (IPublishEndpoint publishEndpoint) : IOutboxMessageSender
    {
        public Task SendAsync(object message, Type messageType, CancellationToken cancellationToken)
            => publishEndpoint.Publish(message, messageType, cancellationToken);
    }
}
