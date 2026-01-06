
using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.OrderIntegrationEvents;

using MediatR;

using OrderService.Domain.Events;

namespace OrderService.Application.DomainEventHandlers
{
    public class OrderItemRemovedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<OrderItemRemovedDomainEvent>
    {
        public async Task Handle(OrderItemRemovedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new OrderItemRemovedIntegrationEvent
            {
                OrderId = notification.OrderId,
                ProductId = notification.ProductId,
                ProductVariantId = notification.ProductVariantId,
                OccurredOn = DateTime.UtcNow
            });
        }
    }
}
