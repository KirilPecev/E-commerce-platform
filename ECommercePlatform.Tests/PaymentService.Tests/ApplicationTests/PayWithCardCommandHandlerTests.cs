using ECommercePlatform.Application.Interfaces;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using PaymentService.Application.Models;
using PaymentService.Application.Payments.Command;
using PaymentService.Domain.Aggregates;
using PaymentService.Domain.Events;
using PaymentService.Domain.ValueObjects;
using PaymentService.Infrastructure.Persistence;

using AppInterfaces = PaymentService.Application.Interfaces;

namespace PaymentService.Tests.ApplicationTests
{
    public class PayWithCardCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenPaymentNotFound()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var gatewayMock = new Mock<AppInterfaces.IPaymentGateway>();
            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new PaymentDbContext(options, dispatcherMock.Object);

            var handler = new PayWithCardCommandHandler(context, gatewayMock.Object);

            Func<Task> act = () => handler.Handle(new PayWithCardCommand(Guid.NewGuid(), "4111", "Holder", "12/25", "123"), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_MarksPaymentAsPaid_WhenGatewaySucceeds_AndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var gatewayMock = new Mock<AppInterfaces.IPaymentGateway>();
            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            gatewayMock.Setup(g => g.ProcessCardPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CardDetails>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentResult(true));

            await using (var context = new PaymentDbContext(options, dispatcherMock.Object))
            {
                var orderId = Guid.NewGuid();
                var payment = new Payment(orderId, new Money(50m, "USD"));

                context.Payments.Add(payment);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                dispatcherMock.Reset();

                var handler = new PayWithCardCommandHandler(context, gatewayMock.Object);

                await handler.Handle(new PayWithCardCommand(payment.Id, "4111111111111111", "Holder", "12/25", "123"), CancellationToken.None);

                var saved = await context.Payments.FirstAsync(p => p.Id == payment.Id, TestContext.Current.CancellationToken);
                saved.Status.Should().Be(PaymentStatus.Paid);
                saved.PaymentMethod.Should().Be(PaymentMethod.Card);
                saved.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<ECommercePlatform.Domain.Events.IDomainEvent>(ev => ev.GetType() == typeof(PaymentCompletedDomainEvent) && ((PaymentCompletedDomainEvent)ev).PaymentId == payment.Id && ((PaymentCompletedDomainEvent)ev).OrderId == orderId)), Times.Once);
            }
        }

        [Fact]
        public async Task Handle_MarksPaymentAsFailed_WhenGatewayFails_AndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var gatewayMock = new Mock<AppInterfaces.IPaymentGateway>();
            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            gatewayMock.Setup(g => g.ProcessCardPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CardDetails>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentResult(false, "declined"));

            await using (var context = new PaymentDbContext(options, dispatcherMock.Object))
            {
                var orderId = Guid.NewGuid();
                var payment = new Payment(orderId, new Money(75m, "USD"));

                context.Payments.Add(payment);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                dispatcherMock.Reset();

                var handler = new PayWithCardCommandHandler(context, gatewayMock.Object);

                await handler.Handle(new PayWithCardCommand(payment.Id, "4111111111111111", "Holder", "12/25", "123"), CancellationToken.None);

                var saved = await context.Payments.FirstAsync(p => p.Id == payment.Id, TestContext.Current.CancellationToken);
                saved.Status.Should().Be(PaymentStatus.Failed);

                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<ECommercePlatform.Domain.Events.IDomainEvent>(ev => ev.GetType() == typeof(PaymentFailedDomainEvent) && ((PaymentFailedDomainEvent)ev).PaymentId == payment.Id && ((PaymentFailedDomainEvent)ev).OrderId == orderId && ((PaymentFailedDomainEvent)ev).FailureReason == "declined")), Times.Once);
            }
        }
    }
}
