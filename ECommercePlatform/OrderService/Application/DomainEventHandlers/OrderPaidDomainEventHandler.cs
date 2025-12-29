using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.OrderIntegrationEvents;

using MediatR;

using OrderService.Domain.Events;

namespace OrderService.Application.DomainEventHandlers
{
    public class OrderPaidDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<OrderPaidDomainEvent>
    {
        public async Task Handle(OrderPaidDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new OrderPaidIntegrationEvent
            {
                OrderId = notification.OrderId,
                OccurredOn = DateTime.UtcNow
            });
        }
    }
}
