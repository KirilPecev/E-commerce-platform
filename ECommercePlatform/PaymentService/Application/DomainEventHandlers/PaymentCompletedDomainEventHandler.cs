
using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.PaymentIntegrationEvents;

using MediatR;

using PaymentService.Domain.Events;

namespace PaymentService.Application.DomainEventHandlers
{
    public class PaymentCompletedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<PaymentCompletedDomainEvent>
    {
        public async Task Handle(PaymentCompletedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new PaymentCompletedIntegrationEvent
            {
                PaymentId = notification.PaymentId,
                OrderId = notification.OrderId,
                OccurredOn = notification.OccurredOn
            });
        }
    }
}
