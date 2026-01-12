
using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.PaymentIntegrationEvents;

using MediatR;

using PaymentService.Domain.Events;

namespace PaymentService.Application.DomainEventHandlers
{
    public class PaymentFailedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<PaymentFailedDomainEvent>
    {
        public async Task Handle(PaymentFailedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new PaymentFailedIntegrationEvent
            {
                PaymentId = notification.PaymentId,
                OrderId = notification.OrderId,
                FailureReason = notification.FailureReason,
                OccurredOn = notification.OccurredOn
            });
        }
    }
}
