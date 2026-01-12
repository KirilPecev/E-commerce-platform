
using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.InventoryIntegrationEvents;

using InventoryService.Domain.Events;

using MediatR;

namespace InventoryService.Application.DomainEventHandlers
{
    public class StockReservedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<StockReservedDomainEvent>
    {
        public async Task Handle(StockReservedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new StockReservedIntegrationEvent
            {
                OrderId = notification.OrderId,
                ProductId = notification.ProductId,
                ProductVariantId = notification.ProductVariantId,
                Quantity = notification.Quantity
            });
        }
    }
}
