
using CatalogService.Application;
using CatalogService.Domain.Events;

using MediatR;

namespace CatalogService.Infrastructure
{
    public class DomainEventDispatcher
        (IMediator mediator) : IDomainEventDispatcher
    {
        public async Task DispatchAsync(IDomainEvent domainEvent)
        {
            await mediator.Publish(domainEvent);
        }
    }
}
