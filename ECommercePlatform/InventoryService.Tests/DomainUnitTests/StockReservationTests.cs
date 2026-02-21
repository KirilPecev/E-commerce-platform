using System;
using FluentAssertions;
using Xunit;
using InventoryService.Domain.Aggregates;

namespace InventoryService.Tests.DomainUnitTests
{
    public class StockReservationTests
    {
        [Fact]
        public void Ctor_SetsProperties()
        {
            var orderId = Guid.NewGuid();
            var reservation = new StockReservation(orderId, 3);

            reservation.OrderId.Should().Be(orderId);
            reservation.Quantity.Should().Be(3);
            reservation.Status.Should().Be(ReservationStatus.Pending);
            reservation.ReservedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Confirm_SetsStatusToConfirmed()
        {
            var reservation = new StockReservation(Guid.NewGuid(), 2);

            reservation.Confirm();

            reservation.Status.Should().Be(ReservationStatus.Confirmed);
        }

        [Fact]
        public void Release_SetsStatusToReleased()
        {
            var reservation = new StockReservation(Guid.NewGuid(), 2);

            reservation.Release();

            reservation.Status.Should().Be(ReservationStatus.Released);
        }
    }
}
