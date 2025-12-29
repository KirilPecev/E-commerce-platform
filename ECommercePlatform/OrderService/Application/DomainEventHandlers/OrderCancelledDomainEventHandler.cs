
using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.OrderIntegrationEvents;

using MediatR;

using OrderService.Domain.Events;

namespace OrderService.Application.DomainEventHandlers
{
    public class OrderCancelledDomainEventHandler
         (IEventPublisher eventPublisher) : INotificationHandler<OrderCancelledDomainEvent>
    {
        public async Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new OrderCancelledIntegrationEvent
            {
                OrderId = notification.OrderId,
                Reason = notification.Reason,
                OccurredOn = notification.OccurredOn
            });
        }
    }
}
