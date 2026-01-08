using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using MediatR;

namespace PaymentService.Infrastructure
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
