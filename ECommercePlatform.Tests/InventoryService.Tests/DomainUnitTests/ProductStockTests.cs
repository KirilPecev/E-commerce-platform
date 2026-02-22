using FluentAssertions;

using InventoryService.Domain.Aggregates;
using InventoryService.Domain.Events;
using InventoryService.Domain.Exceptions;

namespace InventoryService.Tests.DomainUnitTests
{
    public class ProductStockTests
    {
        [Fact]
        public void Ctor_SetsProperties_WhenValid()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            // Act
            var stock = new ProductStock(productId, variantId, 10);

            // Assert
            stock.ProductId.Should().Be(productId);
            stock.ProductVariantId.Should().Be(variantId);
            stock.AvailableQuantity.Should().Be(10);
            stock.Reservations.Should().BeEmpty();
        }

        [Fact]
        public void Ctor_Throws_WhenInitialQuantityNegative()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            // Act
            Action act = () => new ProductStock(productId, variantId, -1);

            // Assert
            act.Should().Throw<InventoryDomainException>()
                .WithMessage("Initial quantity cannot be negative.");
        }

        [Fact]
        public void UpdateQuantity_Updates_WhenValid()
        {
            var stock = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 5);

            stock.UpdateQuantity(20);

            stock.AvailableQuantity.Should().Be(20);
        }

        [Fact]
        public void UpdateQuantity_Throws_WhenNegative()
        {
            var stock = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 5);

            Action act = () => stock.UpdateQuantity(-5);

            act.Should().Throw<InventoryDomainException>()
                .WithMessage("Quantity cannot be negative.");
        }

        [Fact]
        public void Reserve_Throws_WhenQuantityNotPositive()
        {
            var stock = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 5);

            Action act = () => stock.Reserve(Guid.NewGuid(), 0);

            act.Should().Throw<InventoryDomainException>()
                .WithMessage("Quantity must be positive.");
        }

        [Fact]
        public void Reserve_Fails_WhenInsufficientAvailable()
        {
            var stock = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 2);
            var orderId = Guid.NewGuid();

            // try to reserve more than available - should not throw but not create reservation
            stock.Reserve(orderId, 5);

            stock.AvailableQuantity.Should().Be(2);
            stock.Reservations.Should().BeEmpty();
            stock.HasReservedStockForOrder(orderId).Should().BeFalse();

            // Domain event for failed reservation should be added
            stock.DomainEvents.OfType<StockReservationFailedDomainEvent>()
                .Should().ContainSingle(e => e.OrderId == orderId && e.ProductId == stock.ProductId && e.ProductVariantId == stock.ProductVariantId);
        }

        [Fact]
        public void Reserve_Succeeds_WhenSufficientAvailable()
        {
            var stock = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 10);
            var orderId = Guid.NewGuid();

            stock.Reserve(orderId, 4);

            stock.AvailableQuantity.Should().Be(6);
            stock.Reservations.Should().HaveCount(1);
            stock.ReservedQuantity.Should().Be(4);
            stock.HasReservedStockForOrder(orderId).Should().BeTrue();
            stock.HasReservedPendingStockForOrder(orderId).Should().BeTrue();

            // Domain event for successful reservation should be added
            stock.DomainEvents.OfType<StockReservedDomainEvent>()
                .Should().ContainSingle(e => e.OrderId == orderId && e.Quantity == 4 && e.ProductId == stock.ProductId && e.ProductVariantId == stock.ProductVariantId);
        }

        [Fact]
        public void Confirm_ChangesReservationStatus()
        {
            var stock = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 10);
            var orderId = Guid.NewGuid();

            stock.Reserve(orderId, 3);

            stock.Confirm(orderId);

            stock.HasReservedStockForOrder(orderId).Should().BeTrue();
            stock.HasReservedPendingStockForOrder(orderId).Should().BeFalse();
            stock.Reservations.First(r => r.OrderId == orderId).Should().Match<StockReservation>(r => r.Status == ReservationStatus.Confirmed);
        }

        [Fact]
        public void Release_ReleasesReservationAndRestoresQuantity()
        {
            var stock = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 10);
            var orderId = Guid.NewGuid();

            stock.Reserve(orderId, 5);

            stock.Release(orderId);

            stock.AvailableQuantity.Should().Be(10);
            stock.HasReservedStockForOrder(orderId).Should().BeFalse();
            stock.Reservations.First(r => r.OrderId == orderId).Status.Should().Be(ReservationStatus.Released);

            // Domain event for release should be added
            stock.DomainEvents.OfType<StockReleasedDomainEvent>()
                .Any(e => e.OrderId == orderId && e.ProductId == stock.ProductId && e.ProductVariantId == stock.ProductVariantId)
                .Should().BeTrue();
        }

        [Fact]
        public void Confirm_Throws_WhenReservationNotFound()
        {
            var stock = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 10);

            Action act = () => stock.Confirm(Guid.NewGuid());

            act.Should().Throw<InventoryDomainException>()
                .WithMessage("Reservation not found.");
        }

        [Fact]
        public void Release_Throws_WhenReservationNotFound()
        {
            var stock = new ProductStock(Guid.NewGuid(), Guid.NewGuid(), 10);

            Action act = () => stock.Release(Guid.NewGuid());

            act.Should().Throw<InventoryDomainException>()
                .WithMessage("Reservation not found.");
        }
    }
}
