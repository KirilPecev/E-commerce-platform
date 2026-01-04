using CatalogService.Domain.Events;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.ProductIntegrationEvents;

using MediatR;


namespace CatalogService.Application.DomainEventHandlers
{
    public class ProductCreatedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<ProductCreatedDomainEvent>
    {
        public async Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new ProductCreatedIntegrationEvent
            {
                ProductId = notification.ProductId,
                ProductVariantId = notification.ProductVariantId,
                InitialQuantity = notification.InitialQuantity,
                OccurredOn = notification.OccurredOn
            });
        }
    }
}
