using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Moq;
using Xunit;

using InventoryService.Infrastructure.Messaging.Consumers;
using InventoryService.Application.Inventory.Commands;
using ECommercePlatform.Events.OrderIntegrationEvents;

namespace InventoryService.Tests.ApplicationTests
{
    public class OrderCreatedIntegrationEventConsumerTests
    {
        [Fact]
        public async Task Consume_SendsReserveStockCommand_WithCorrectValues()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<ReserveStockCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));

            var consumer = new OrderCreatedIntegrationEventConsumer(mediatorMock.Object);

            var message = new OrderCreatedIntegrationEvent
            {
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                Quantity = 5,
                OccurredOn = DateTime.UtcNow
            };

            var contextMock = new Mock<ConsumeContext<OrderCreatedIntegrationEvent>>();
            contextMock.SetupGet(c => c.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            mediatorMock.Verify(m => m.Send(It.Is<ReserveStockCommand>(c => c.OrderId == message.OrderId && c.ProductId == message.ProductId && c.ProductVariantId == message.ProductVariantId && c.Quantity == message.Quantity), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
