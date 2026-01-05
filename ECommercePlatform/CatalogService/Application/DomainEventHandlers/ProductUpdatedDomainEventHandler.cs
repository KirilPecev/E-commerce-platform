using CatalogService.Domain.Events;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.ProductIntegrationEvents;

using MediatR;


namespace CatalogService.Application.DomainEventHandlers
{
    public class ProductUpdatedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<ProductUpdatedDomainEvent>
    {
        public async Task Handle(ProductUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new ProductUpdatedIntegrationEvent
            {
                ProductId = notification.ProductId,
                ProductVariantId = notification.ProductVariantId,
                Quantity = notification.Quantity,
                OccurredOn = notification.OccurredOn
            });
        }
    }
}
