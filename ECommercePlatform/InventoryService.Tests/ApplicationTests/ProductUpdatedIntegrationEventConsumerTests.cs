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
    public class ProductUpdatedIntegrationEventConsumerTests
    {
        [Fact]
        public async Task Consume_SendsUpdateProductStockCommand_WithCorrectValues()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<UpdateProductStockCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));

            var consumer = new ProductUpdatedIntegrationEventConsumer(mediatorMock.Object);

            var message = new ProductUpdatedIntegrationEvent
            {
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                Quantity = 42,
                OccurredOn = DateTime.UtcNow
            };

            var contextMock = new Mock<ConsumeContext<ProductUpdatedIntegrationEvent>>();
            contextMock.SetupGet(c => c.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            mediatorMock.Verify(m => m.Send(It.Is<UpdateProductStockCommand>(c => c.ProductId == message.ProductId && c.ProductVariantId == message.ProductVariantId && c.Quantity == message.Quantity), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
