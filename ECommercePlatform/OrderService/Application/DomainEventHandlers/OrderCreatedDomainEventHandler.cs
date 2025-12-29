using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.OrderIntegrationEvents;

using MediatR;

using OrderService.Domain.Events;

namespace OrderService.Application.DomainEventHandlers
{
    public class OrderCreatedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<OrderCreatedDomainEvent>
    {
        public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new OrderCreatedIntegrationEvent
            {
                OrderId = notification.OrderId,
                OccurredOn = notification.OccurredOn
            });
        }
    }
}
