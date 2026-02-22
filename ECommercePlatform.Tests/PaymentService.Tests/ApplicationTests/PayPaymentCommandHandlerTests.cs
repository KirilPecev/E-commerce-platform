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
    public class PayPaymentCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenPaymentNotFound()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new PaymentDbContext(options, dispatcherMock.Object);

            var handler = new PayPaymentCommandHandler(context);

            Func<Task> act = () => handler.Handle(new PayPaymentCommand(Guid.NewGuid()), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_Throws_WhenNotPending()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new PaymentDbContext(options, dispatcherMock.Object);
            var payment = new Payment(Guid.NewGuid(), new Money(10m, "USD"));
            payment.MarkAsPaid(PaymentMethod.Card);

            context.Payments.Add(payment);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new PayPaymentCommandHandler(context);

            Func<Task> act = () => handler.Handle(new PayPaymentCommand(payment.Id), CancellationToken.None);

            await act.Should().ThrowAsync<PaymentDomainException>().WithMessage("Payment is not pending.");
        }

        [Fact]
        public async Task Handle_MarksPaymentAsPaid_AndDispatchesEvent()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new PaymentDbContext(options, dispatcherMock.Object))
            {
                var orderId = Guid.NewGuid();
                var payment = new Payment(orderId, new Money(25m, "USD"));

                context.Payments.Add(payment);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                // reset mock in case any events were dispatched (shouldn't be)
                dispatcherMock.Reset();

                var handler = new PayPaymentCommandHandler(context);

                await handler.Handle(new PayPaymentCommand(payment.Id), CancellationToken.None);

                var saved = await context.Payments.FirstAsync(p => p.Id == payment.Id, TestContext.Current.CancellationToken);
                saved.Status.Should().Be(PaymentStatus.Paid);
                saved.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

                dispatcherMock.Verify(d => d.DispatchAsync(It.Is<IDomainEvent>(ev => ev.GetType() == typeof(PaymentCompletedDomainEvent) && ((PaymentCompletedDomainEvent)ev).PaymentId == payment.Id && ((PaymentCompletedDomainEvent)ev).OrderId == orderId)), Times.Once);
            }
        }
    }
}
