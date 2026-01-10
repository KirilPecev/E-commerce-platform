using ECommercePlatform.Domain.Abstractions;

using PaymentService.Domain.Events;
using PaymentService.Domain.Exceptions;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Domain.Aggregates
{
    public class Payment : AggregateRoot
    {
        public Guid OrderId { get; private set; }
        public Money Amount { get; private set; }
        public PaymentStatus Status { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        public DateTime ProcessedAt { get; private set; }

        private Payment()
        {
            Amount = default!;
        }

        public Payment(Guid orderId, Money amount)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            Amount = amount;
            Status = PaymentStatus.Pending;
        }

        public void MarkAsPaid(PaymentMethod paymentMethod)
        {
            if (Status != PaymentStatus.Pending)
                throw new PaymentDomainException("Payment is not pending.");

            PaymentMethod = paymentMethod;
            Status = PaymentStatus.Paid;
            ProcessedAt = DateTime.UtcNow;

            AddDomainEvent(
                new PaymentCompletedDomainEvent(Id, OrderId));
        }

        public void MarkAsFailed()
        {
            if (Status != PaymentStatus.Pending)
                throw new PaymentDomainException("Payment is not pending.");

            Status = PaymentStatus.Failed;

            AddDomainEvent(
                new PaymentFailedDomainEvent(Id, OrderId));
        }

        public void Refund()
        {
            if (Status != PaymentStatus.Paid)
                throw new PaymentDomainException("Payment is not paid.");

            Status = PaymentStatus.Refunded;

            AddDomainEvent(
                new PaymentRefundedDomainEvent(Id, OrderId));
        }
    }
}