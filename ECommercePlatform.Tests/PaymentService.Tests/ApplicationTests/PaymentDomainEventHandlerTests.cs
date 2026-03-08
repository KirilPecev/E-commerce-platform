using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.PaymentIntegrationEvents;

using Moq;

using PaymentService.Application.DomainEventHandlers;
using PaymentService.Domain.Events;

namespace PaymentService.Tests.ApplicationTests
{
    public class PaymentDomainEventHandlerTests
    {
        private readonly Mock<IEventPublisher> publisherMock = new();

        [Fact]
        public async Task PaymentCompletedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new PaymentCompletedDomainEventHandler(publisherMock.Object);

            var paymentId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var domainEvent = new PaymentCompletedDomainEvent(paymentId, orderId);

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<PaymentCompletedIntegrationEvent>(e =>
                e.PaymentId == paymentId &&
                e.OrderId == orderId)), Times.Once);
        }

        [Fact]
        public async Task PaymentFailedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new PaymentFailedDomainEventHandler(publisherMock.Object);

            var paymentId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var domainEvent = new PaymentFailedDomainEvent(paymentId, orderId, "Insufficient funds");

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<PaymentFailedIntegrationEvent>(e =>
                e.PaymentId == paymentId &&
                e.OrderId == orderId &&
                e.FailureReason == "Insufficient funds")), Times.Once);
        }

        [Fact]
        public async Task PaymentRefundedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new PaymentRefundedDomainEventHandler(publisherMock.Object);

            var paymentId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var domainEvent = new PaymentRefundedDomainEvent(paymentId, orderId);

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<PaymentRefundedIntegrationEvent>(e =>
                e.PaymentId == paymentId &&
                e.OrderId == orderId)), Times.Once);
        }
    }
}
