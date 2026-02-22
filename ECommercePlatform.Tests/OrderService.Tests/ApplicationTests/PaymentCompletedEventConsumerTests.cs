using ECommercePlatform.Events.PaymentIntegrationEvents;

using MassTransit;

using MediatR;

using Moq;

using OrderService.Application.Orders.Commands;
using OrderService.Infrastructure.Messaging.Consumers;

namespace OrderService.Tests.ApplicationTests
{
    public class PaymentCompletedEventConsumerTests
    {
        [Fact]
        public async Task Consume_SendsPayOrderCommand_WithCorrectOrderId()
        {
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<PayOrderCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Unit.Value));

            var consumer = new PaymentCompletedEventConsumer(mediatorMock.Object);

            var message = new PaymentCompletedIntegrationEvent
            {
                PaymentId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                OccurredOn = DateTime.UtcNow
            };

            var contextMock = new Mock<ConsumeContext<PaymentCompletedIntegrationEvent>>();
            contextMock.SetupGet(c => c.Message).Returns(message);

            await consumer.Consume(contextMock.Object);

            mediatorMock.Verify(m => m.Send(
                It.Is<PayOrderCommand>(cmd => cmd.OrderId == message.OrderId),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
