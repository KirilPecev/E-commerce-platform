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
    public class OrderItemRemovedIntegrationEventConsumerTests
    {
        [Fact]
        public async Task Consume_SendsReleaseStockCommand_WithCorrectValues()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock.Setup(m => m.Send(It.IsAny<ReleaseStockCommand>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Unit.Value));

            var consumer = new OrderItemRemovedIntegrationEventConsumer(mediatorMock.Object);

            var message = new OrderItemRemovedIntegrationEvent
            {
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductVariantId = Guid.NewGuid(),
                OccurredOn = DateTime.UtcNow
            };

            var contextMock = new Mock<ConsumeContext<OrderItemRemovedIntegrationEvent>>();
            contextMock.SetupGet(c => c.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            mediatorMock.Verify(m => m.Send(
                It.Is<ReleaseStockCommand>(cmd => cmd.ProductId == message.ProductId && cmd.ProductVariantId == message.ProductVariantId && cmd.OrderId == message.OrderId),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
