using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.InventoryIntegrationEvents;

using InventoryService.Application.DomainEventHandlers;
using InventoryService.Domain.Events;

using Moq;

namespace InventoryService.Tests.ApplicationTests
{
    public class InventoryDomainEventHandlerTests
    {
        private readonly Mock<IEventPublisher> publisherMock = new();

        [Fact]
        public async Task StockReservedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new StockReservedDomainEventHandler(publisherMock.Object);

            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var domainEvent = new StockReservedDomainEvent(orderId, productId, variantId, 5);

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<StockReservedIntegrationEvent>(e =>
                e.OrderId == orderId &&
                e.ProductId == productId &&
                e.ProductVariantId == variantId &&
                e.Quantity == 5)), Times.Once);
        }

        [Fact]
        public async Task StockReleasedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new StockReleasedDomainEventHandler(publisherMock.Object);

            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var domainEvent = new StockReleasedDomainEvent(orderId, productId, variantId);

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<StockReleasedIntegrationEvent>(e =>
                e.OrderId == orderId &&
                e.ProductId == productId &&
                e.ProductVariantId == variantId)), Times.Once);
        }

        [Fact]
        public async Task StockReservationFailedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new StockReservationFailedDomainEventHandler(publisherMock.Object);

            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var domainEvent = new StockReservationFailedDomainEvent(orderId, productId, variantId, "Not enough stock");

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<StockReservationFailedIntegrationEvent>(e =>
                e.OrderId == orderId &&
                e.ProductId == productId &&
                e.ProductVariantId == variantId &&
                e.Reason == "Not enough stock")), Times.Once);
        }
    }
}
