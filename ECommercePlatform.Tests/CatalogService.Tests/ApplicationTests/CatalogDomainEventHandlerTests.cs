using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Events.ProductIntegrationEvents;

using CatalogService.Application.DomainEventHandlers;
using CatalogService.Domain.Events;

using Moq;

namespace CatalogService.Tests.ApplicationTests
{
    public class CatalogDomainEventHandlerTests
    {
        private readonly Mock<IEventPublisher> publisherMock = new();

        [Fact]
        public async Task ProductCreatedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new ProductCreatedDomainEventHandler(publisherMock.Object);

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var domainEvent = new ProductCreatedDomainEvent(productId, variantId, 100);

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<ProductCreatedIntegrationEvent>(e =>
                e.ProductId == productId &&
                e.ProductVariantId == variantId &&
                e.InitialQuantity == 100)), Times.Once);
        }

        [Fact]
        public async Task ProductUpdatedHandler_PublishesIntegrationEvent_WithCorrectValues()
        {
            var handler = new ProductUpdatedDomainEventHandler(publisherMock.Object);

            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var domainEvent = new ProductUpdatedDomainEvent(productId, variantId, 50);

            await handler.Handle(domainEvent, CancellationToken.None);

            publisherMock.Verify(p => p.PublishAsync(It.Is<ProductUpdatedIntegrationEvent>(e =>
                e.ProductId == productId &&
                e.ProductVariantId == variantId &&
                e.Quantity == 50)), Times.Once);
        }
    }
}
