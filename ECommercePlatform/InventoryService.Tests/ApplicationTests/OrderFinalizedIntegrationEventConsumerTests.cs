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
    public class OrderFinalizedIntegrationEventConsumerTests
    {
        [Fact]
        public async Task Consume_SendsConfirmStockCommand_WithCorrectOrderId()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<ConfirmStockCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));

            var consumer = new OrderFinalizedIntegrationEventConsumer(mediatorMock.Object);

            var message = new OrderFinalizedIntegrationEvent
            {
                OrderId = Guid.NewGuid(),
                OccurredOn = DateTime.UtcNow,
                Amount = 10m,
                Currecy = "USD"
            };

            var contextMock = new Mock<ConsumeContext<OrderFinalizedIntegrationEvent>>();
            contextMock.SetupGet(c => c.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            mediatorMock.Verify(m => m.Send(It.Is<ConfirmStockCommand>(c => c.OrderId == message.OrderId), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
