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
    public class OrderCancelledIntegrationEventConsumerTests
    {
        [Fact]
        public async Task Consume_SendsReleaseStocksForOrderCommand_WithCorrectValues()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<ReleaseStocksForOrderCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));

            var consumer = new OrderCancelledIntegrationEventConsumer(mediatorMock.Object);

            var message = new OrderCancelledIntegrationEvent
            {
                OrderId = Guid.NewGuid(),
                Reason = "Customer cancelled",
                OccurredOn = DateTime.UtcNow
            };

            var contextMock = new Mock<ConsumeContext<OrderCancelledIntegrationEvent>>();
            contextMock.SetupGet(c => c.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            mediatorMock.Verify(m => m.Send(It.Is<ReleaseStocksForOrderCommand>(c => c.OrderId == message.OrderId && c.Reason == message.Reason), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
