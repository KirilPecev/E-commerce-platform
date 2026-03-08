using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.OrderIntegrationEvents;

using Moq;

using OrderService.Application.DomainEventHandlers;
using OrderService.Domain.Events;

namespace OrderService.Tests.ApplicationTests
{
    public class OrderDomainEventHandlerTests
    {
        private readonly Mock<IEventPublisher> publisherMock = new();

        [Fact]
        public async Task OrderCreatedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new OrderCreatedDomainEventHandler(publisherMock.Object);

            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var domainEvent = new OrderCreatedDomainEvent(orderId, productId, variantId, 3);

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<OrderCreatedIntegrationEvent>(e =>
                e.OrderId == orderId &&
                e.ProductId == productId &&
                e.ProductVariantId == variantId &&
                e.Quantity == 3)), Times.Once);
        }

        [Fact]
        public async Task OrderFinalizedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new OrderFinalizedDomainEventHandler(publisherMock.Object);

            var orderId = Guid.NewGuid();
            var domainEvent = new OrderFinalizedDomainEvent(orderId, 250.00m, "USD");

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<OrderFinalizedIntegrationEvent>(e =>
                e.OrderId == orderId &&
                e.Amount == 250.00m &&
                e.Currecy == "USD")), Times.Once);
        }

        [Fact]
        public async Task OrderCancelledHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new OrderCancelledDomainEventHandler(publisherMock.Object);

            var orderId = Guid.NewGuid();
            var domainEvent = new OrderCancelledDomainEvent(orderId, "Out of stock");

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<OrderCancelledIntegrationEvent>(e =>
                e.OrderId == orderId &&
                e.Reason == "Out of stock")), Times.Once);
        }

        [Fact]
        public async Task OrderItemRemovedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new OrderItemRemovedDomainEventHandler(publisherMock.Object);

            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var domainEvent = new OrderItemRemovedDomainEvent(orderId, productId, variantId);

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<OrderItemRemovedIntegrationEvent>(e =>
                e.OrderId == orderId &&
                e.ProductId == productId &&
                e.ProductVariantId == variantId)), Times.Once);
        }

        [Fact]
        public async Task OrderShippedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new OrderShippedDomainEventHandler(publisherMock.Object);

            var orderId = Guid.NewGuid();
            var domainEvent = new OrderShippedDomainEvent(orderId, "TRACK-12345");

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<OrderShippedIntegrationEvent>(e =>
                e.OrderId == orderId &&
                e.TrackingNumber == "TRACK-12345")), Times.Once);
        }
    }
}
