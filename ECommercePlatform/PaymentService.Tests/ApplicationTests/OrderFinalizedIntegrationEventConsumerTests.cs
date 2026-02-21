using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Moq;
using Xunit;

using PaymentService.Infrastructure.Messaging.Consumers;
using PaymentService.Application.Payments.Command;
using ECommercePlatform.Events.OrderIntegrationEvents;

namespace PaymentService.Tests.ApplicationTests
{
    public class OrderFinalizedIntegrationEventConsumerTests
    {
        [Fact]
        public async Task Consume_SendsCreatePaymentCommand_WithCorrectValues()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            var consumer = new OrderFinalizedIntegrationEventConsumer(mediatorMock.Object);

            var message = new OrderFinalizedIntegrationEvent
            {
                OrderId = Guid.NewGuid(),
                OccurredOn = DateTime.UtcNow,
                Amount = 123.45m,
                Currecy = "USD"
            };

            var contextMock = new Mock<ConsumeContext<OrderFinalizedIntegrationEvent>>();
            contextMock.SetupGet(c => c.Message).Returns(message);

            // Act
            await consumer.Consume(contextMock.Object);

            // Assert
            mediatorMock.Verify(m => m.Send(
                It.Is<CreatePaymentCommand>(cmd => cmd.OrderId == message.OrderId && cmd.Amount == message.Amount && cmd.Currency == message.Currecy),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
