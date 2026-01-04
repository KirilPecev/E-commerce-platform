using ECommercePlatform.Domain.Abstractions;

using InventoryService.Domain.Events;
using InventoryService.Domain.Exceptions;

namespace InventoryService.Domain.Aggregates
{
    public class ProductStock : AggregateRoot
    {
        public Guid ProductId { get; private set; }
        public Guid ProductVariantId { get; private set; }
        public int AvailableQuantity { get; private set; }
        public List<StockReservation> Reservations { get; private set; } = new();

        private ProductStock() { }

        public ProductStock(Guid productId, Guid productVariantId, int initialQuantity)
        {
            if (initialQuantity < 0)
                throw new InventoryDomainException("Initial quantity cannot be negative.");

            Id = Guid.NewGuid();
            ProductId = productId;
            ProductVariantId = productVariantId;
            AvailableQuantity = initialQuantity;
        }

        public void Reserve(Guid orderId, int quantity)
        {
            if (quantity <= 0)
                throw new InventoryDomainException("Quantity must be positive.");

            if (AvailableQuantity < quantity)
            {
                AddDomainEvent(new StockReservationFailedDomainEvent(orderId, ProductId, ProductVariantId, "No available quantity."));

                return;
            }

            AvailableQuantity -= quantity;

            Reservations.Add(new StockReservation(orderId, quantity));

            AddDomainEvent(new StockReservedDomainEvent(
                orderId, ProductId, ProductVariantId, quantity));
        }

        public void Release(Guid orderId)
        {
            StockReservation? reservation = Reservations
                .FirstOrDefault(r => r.OrderId == orderId);

            if (reservation is null)
                throw new InventoryDomainException("Reservation not found.");

            AvailableQuantity += reservation.Quantity;
            reservation.Release();

            AddDomainEvent(new StockReleasedDomainEvent(
                orderId, ProductId, ProductVariantId));
        }

        public void Confirm(Guid orderId)
        {
            StockReservation? reservation = Reservations
                .FirstOrDefault(r => r.OrderId == orderId);

            if (reservation is null)
                throw new InventoryDomainException("Reservation not found.");

            reservation.Confirm();
        }
    }
}
