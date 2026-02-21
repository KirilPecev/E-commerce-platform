using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Domain.Events;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using OrderService.Application.Orders.Commands;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Tests.ApplicationTests
{
    public class SetOrderAddressCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenOrderNotFound()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var handler = new SetOrderAddressCommandHandler(context);

            Func<Task> act = () => handler.Handle(new SetOrderAddressCommand(Guid.NewGuid(), "S", "C", "Z", "Country"), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_SetsAddress_WhenOrderIsDraft()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var order = new Order(Guid.NewGuid());
                context.Orders.Add(order);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                var handler = new SetOrderAddressCommandHandler(context);

                await handler.Handle(new SetOrderAddressCommand(order.Id, "Street 1", "City", "12345", "Country"), CancellationToken.None);

                var saved = await context.Orders.FirstAsync(o => o.Id == order.Id, TestContext.Current.CancellationToken);
                saved.ShippingAddress.Should().NotBeNull();
                saved.ShippingAddress!.Street.Should().Be("Street 1");
                saved.ShippingAddress.City.Should().Be("City");
                saved.ShippingAddress.ZipCode.Should().Be("12345");
                saved.ShippingAddress.Country.Should().Be("Country");

                // setting address does not dispatch domain events
                dispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IDomainEvent>()), Times.Never);
            }
        }

        [Fact]
        public async Task Handle_Throws_WhenOrderNotDraft()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using (var context = new OrdersDbContext(options, dispatcherMock.Object))
            {
                var order = new Order(Guid.NewGuid());
                order.AddItem(Guid.NewGuid(), Guid.NewGuid(), "P", new Money(1m, "USD"), 1);
                order.SetShippingAddress(new Address("St", "C", "Z", "Country"));
                order.FinalizeOrder();

                context.Orders.Add(order);
                await context.SaveChangesAsync(TestContext.Current.CancellationToken);

                var handler = new SetOrderAddressCommandHandler(context);

                Func<Task> act = () => handler.Handle(new SetOrderAddressCommand(order.Id, "New St", "C2", "Z2", "Country2"), CancellationToken.None);

                await act.Should().ThrowAsync<OrderDomainException>().WithMessage("Cannot change address after payment.");
            }
        }

        [Fact]
        public async Task Handle_Throws_WhenAddressInvalid()
        {
            var options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dispatcherMock = new Mock<IDomainEventDispatcher>();

            await using var context = new OrdersDbContext(options, dispatcherMock.Object);

            var order = new Order(Guid.NewGuid());
            context.Orders.Add(order);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new SetOrderAddressCommandHandler(context);

            Func<Task> act = () => handler.Handle(new SetOrderAddressCommand(order.Id, "", "C", "Z", "Country"), CancellationToken.None);

            await act.Should().ThrowAsync<OrderDomainException>().WithMessage("Street is required.");
        }
    }
}
