using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using PaymentService.Application.Payments.Command;
using PaymentService.Domain.Aggregates;
using PaymentService.Domain.Events;
using PaymentService.Domain.Exceptions;
using PaymentService.Domain.ValueObjects;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Tests.ApplicationTests
{
    public class RefundPaymentCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenPaymentNotFound()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new PaymentDbContext(options, dispatcherMock.Object);

            var handler = new RefundPaymentCommandHandler(context);

            Func<Task> act = () => handler.Handle(new RefundPaymentCommand(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_Throws_WhenNotPaid()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new PaymentDbContext(options, dispatcherMock.Object);
            var payment = new Payment(Guid.NewGuid(), new Money(10m, "USD"));

            context.Payments.Add(payment);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new RefundPaymentCommandHandler(context);

            Func<Task> act = () => handler.Handle(new RefundPaymentCommand(payment.Id), CancellationToken.None);

            await act.Should().ThrowAsync<PaymentDomainException>().WithMessage("Payment is not paid.");
        }

        [Fact]
        public async Task Handle_RefundsPaymentAndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new PaymentDbContext(options, dispatcherMock.Object))
            {
                var orderId = Guid.NewGuid();
                var payment = new Payment(orderId, new Money(40m, "USD"));

                // mark as paid so refund is allowed
                payment.MarkAsPaid(PaymentMethod.Card);

                context.Payments.Add(payment);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                dispatcherMock.Reset();

                var handler = new RefundPaymentCommandHandler(context);

                await handler.Handle(new RefundPaymentCommand(payment.Id), CancellationToken.None);

                var saved = await context.Payments.FirstAsync(p => p.Id == payment.Id, TestContext.Current.CancellationToken);
                saved.Status.Should().Be(PaymentStatus.Refunded);

                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(PaymentRefundedDomainEvent) && ((PaymentRefundedDomainEvent)ev).PaymentId == payment.Id && ((PaymentRefundedDomainEvent)ev).OrderId == orderId)), Times.Once);
            }
        }
    }
}
