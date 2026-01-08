
using ECommercePlatform.Domain.Events;

using MediatR;

namespace PaymentService.Domain.Events
{
    public class PaymentRefundedDomainEvent : IDomainEvent, INotification
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public PaymentRefundedDomainEvent(Guid paymentId, Guid orderId)
        {
            PaymentId = paymentId;
            OrderId = orderId;
        }
    }
}
