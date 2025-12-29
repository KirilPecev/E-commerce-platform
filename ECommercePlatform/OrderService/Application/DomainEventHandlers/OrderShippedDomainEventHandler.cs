using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.OrderIntegrationEvents;

using MediatR;

using OrderService.Domain.Events;

namespace OrderService.Application.DomainEventHandlers
{
    public class OrderShippedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<OrderShippedDomainEvent>
    {
        public async Task Handle(OrderShippedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new OrderShippedIntegrationEvent
            {
                OrderId = notification.OrderId,
                TrackingNumber = notification.TrackingNumber,
                OccurredOn = notification.OccurredOn
            });
        }
    }
}
