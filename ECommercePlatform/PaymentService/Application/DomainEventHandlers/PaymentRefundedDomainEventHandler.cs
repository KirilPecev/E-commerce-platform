
using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.PaymentIntegrationEvents;

using MediatR;

using PaymentService.Domain.Events;

namespace PaymentService.Application.DomainEventHandlers
{
    public class PaymentRefundedDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<PaymentRefundedDomainEvent>
    {
        public async Task Handle(PaymentRefundedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new PaymentRefundedIntegrationEvent
            {
                PaymentId = notification.PaymentId,
                OrderId = notification.OrderId,
                OccurredOn = notification.OccurredOn
            });
        }
    }
}
