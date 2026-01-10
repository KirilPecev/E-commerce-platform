using ECommercePlatform.Domain.Events;

using MediatR;

namespace PaymentService.Domain.Events
{
    public class PaymentFailedDomainEvent : IDomainEvent, INotification
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public string? FailureReason { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public PaymentFailedDomainEvent(Guid paymentId, Guid orderId, string? failureReason)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            FailureReason = failureReason;
        }
    }
}
