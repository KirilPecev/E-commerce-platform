
using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.InventoryIntegrationEvents;

using InventoryService.Domain.Events;

using MediatR;

namespace InventoryService.Application.DomainEventHandlers
{
    public class StockReleasedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<StockReleasedDomainEvent>
    {
        public async Task Handle(StockReleasedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new StockReleasedIntegrationEvent
            {
                OrderId = notification.OrderId,
                ProductId = notification.ProductId,
                ProductVariantId = notification.ProductVariantId
            });
        }
    }
}
