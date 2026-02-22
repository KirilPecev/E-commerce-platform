using FluentAssertions;

using PaymentService.Domain.Aggregates;
using PaymentService.Domain.Events;
using PaymentService.Domain.Exceptions;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Tests.DomainUnitTests
{
    public class PaymentTests
    {
        [Fact]
        public void Ctor_SetsProperties()
        {
            var orderId = Guid.NewGuid();
            var money = new Money(25.5m, "USD");

            var payment = new Payment(orderId, money);

            payment.OrderId.Should().Be(orderId);
            payment.Amount.Amount.Should().Be(25.5m);
            payment.Amount.Currency.Should().Be("USD");
            payment.Status.Should().Be(PaymentStatus.Pending);
        }

        [Fact]
        public void MarkAsPaid_SetsStatusAndAddsEvent()
        {
            var orderId = Guid.NewGuid();
            var payment = new Payment(orderId, new Money(10m, "EUR"));

            payment.MarkAsPaid(PaymentMethod.Card);

            payment.Status.Should().Be(PaymentStatus.Paid);
            payment.PaymentMethod.Should().Be(PaymentMethod.Card);
            payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            payment.DomainEvents.Should().ContainSingle(ev => ev.GetType() == typeof(PaymentCompletedDomainEvent) && ((PaymentCompletedDomainEvent)ev).PaymentId == payment.Id && ((PaymentCompletedDomainEvent)ev).OrderId == orderId);
        }

        [Fact]
        public void MarkAsPaid_Throws_WhenNotPending()
        {
            var payment = new Payment(Guid.NewGuid(), new Money(5m, "USD"));

            payment.MarkAsFailed("insufficient funds");

            Action act = () => payment.MarkAsPaid(PaymentMethod.BankTransfer);

            act.Should().Throw<PaymentDomainException>().WithMessage("Payment is not pending.");
        }

        [Fact]
        public void MarkAsFailed_SetsFailedAndAddsEvent()
        {
            var orderId = Guid.NewGuid();
            var payment = new Payment(orderId, new Money(8m, "USD"));

            payment.MarkAsFailed("card declined");

            payment.Status.Should().Be(PaymentStatus.Failed);

            payment.DomainEvents.Should().ContainSingle(ev => ev.GetType() == typeof(PaymentFailedDomainEvent) && ((PaymentFailedDomainEvent)ev).PaymentId == payment.Id && ((PaymentFailedDomainEvent)ev).OrderId == orderId && ((PaymentFailedDomainEvent)ev).FailureReason == "card declined");
        }

        [Fact]
        public void MarkAsFailed_Throws_WhenNotPending()
        {
            var payment = new Payment(Guid.NewGuid(), new Money(2m, "USD"));

            payment.MarkAsPaid(PaymentMethod.Courier);

            Action act = () => payment.MarkAsFailed("too late");

            act.Should().Throw<PaymentDomainException>().WithMessage("Payment is not pending.");
        }

        [Fact]
        public void Refund_SetsRefundedAndAddsEvent()
        {
            var orderId = Guid.NewGuid();
            var payment = new Payment(orderId, new Money(30m, "USD"));

            payment.MarkAsPaid(PaymentMethod.Card);

            payment.Refund();

            payment.Status.Should().Be(PaymentStatus.Refunded);

            payment.DomainEvents.Should().Contain(ev => ev.GetType() == typeof(PaymentRefundedDomainEvent) && ((PaymentRefundedDomainEvent)ev).PaymentId == payment.Id && ((PaymentRefundedDomainEvent)ev).OrderId == orderId);
        }

        [Fact]
        public void Refund_Throws_WhenNotPaid()
        {
            var payment = new Payment(Guid.NewGuid(), new Money(30m, "USD"));

            Action act = () => payment.Refund();

            act.Should().Throw<PaymentDomainException>().WithMessage("Payment is not paid.");
        }
    }
}
