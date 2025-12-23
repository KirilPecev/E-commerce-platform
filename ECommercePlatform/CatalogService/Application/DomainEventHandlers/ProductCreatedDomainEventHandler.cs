using CatalogService.Application.Interfaces;
using CatalogService.Domain.Events;

using ECommercePlatform.Events;

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
                ProductId = notification.ProductId
            });
        }
    }
}
