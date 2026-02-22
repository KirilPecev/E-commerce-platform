using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Moq;
using Xunit;

using InventoryService.Infrastructure.Messaging.Consumers;
using InventoryService.Application.Inventory.Commands;
using ECommercePlatform.Events.ProductIntegrationEvents;

namespace InventoryService.Tests.ApplicationTests
{
    public class ProductCreatedIntegrationEventConsumerTests
    {
        [Fact]
        public async Task Consume_SendsCreateProductStockCommand_WithCorrectValues()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<CreateProductStockCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());

            var consumer = new ProductCreatedIntegrationEventConsumer(mediatorMock.Object);

            var message = new ProductCreatedIntegrationEvent
            {
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                InitialQuantity = 11,
                OccurredOn = DateTime.UtcNow
            };

            var contextMock = new Mock<ConsumeContext<ProductCreatedIntegrationEvent>>();
            contextMock.SetupGet(c => c.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            mediatorMock.Verify(m => m.Send(It.Is<CreateProductStockCommand>(c => c.ProductId == message.ProductId && c.ProductVariantId == message.ProductVariantId && c.InitialQuantity == message.InitialQuantity), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
