
using CatalogService.Domain.Events;

using MediatR;

namespace CatalogService.Application
{
    public class ProductCreatedDomainEventHandler
        (IEventPublisher eventPublisher): INotificationHandler<ProductCreatedDomainEvent>
    {
        public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
