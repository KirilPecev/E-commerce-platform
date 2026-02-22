using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using PaymentService.Application.Payments.Command;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Tests.ApplicationTests
{
    public class CreatePaymentCommandHandlerTests
    {
        [Fact]
        public async Task Handle_CreatesPaymentAndPersists()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new PaymentDbContext(options, dispatcherMock.Object);

            var handler = new CreatePaymentCommandHandler(context);

            var orderId = Guid.NewGuid();
            var cmd = new CreatePaymentCommand(orderId, 19.99m, "USD");

            var paymentId = await handler.Handle(cmd, CancellationToken.None);

            paymentId.Should().NotBe(Guid.Empty);

            var saved = await context.Payments.FindAsync(new object[] { paymentId }, CancellationToken.None);
            saved.Should().NotBeNull();
            saved!.OrderId.Should().Be(orderId);
            saved.Amount.Amount.Should().Be(19.99m);
            saved.Amount.Currency.Should().Be("USD");

            // No domain events expected on creation
            dispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IDomainEvent>()), Times.Never);
        }
    }
}
