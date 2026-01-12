
using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.InventoryIntegrationEvents;

using InventoryService.Domain.Events;

using MediatR;

namespace InventoryService.Application.DomainEventHandlers
{
    public class StockReservationFailedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<StockReservationFailedDomainEvent>
    {
        public async Task Handle(StockReservationFailedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new StockReservationFailedIntegrationEvent
            {
                OrderId = notification.OrderId,
                ProductId = notification.ProductId,
                ProductVariantId = notification.ProductVariantId,
                Reason = notification.Reason
            });
        }
    }
}
