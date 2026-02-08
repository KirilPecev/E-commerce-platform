using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.OrderIntegrationEvents;

using MediatR;

using OrderService.Domain.Events;

namespace OrderService.Application.DomainEventHandlers
{
    public class OrderFinalizedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<OrderFinalizedDomainEvent>
    {
        public async Task Handle(OrderFinalizedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new OrderFinalizedIntegrationEvent
            {
                OrderId = notification.OrderId,
                Amount = notification.TotalPrice,
                Currecy = notification.Currency,
                OccurredOn = DateTime.UtcNow
            });
        }
    }
}
