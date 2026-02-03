using ECommercePlatform.Domain.Abstractions;

using OrderService.Domain.Events;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Aggregates
{
    public class Order : AggregateRoot
    {
        public Guid CustomerId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public OrderStatus Status { get; private set; }
        public decimal TotalPrice { get; private set; }
        public Address? ShippingAddress { get; private set; } = default!;
        public string? CancellationReason { get; private set; }
        public DateTime? ShippedAt { get; private set; }
        public string? TrackingNumber { get; private set; }
        public List<OrderItem> Items { get; private set; } = new();

        // Initialize non-nullable properties with default! to satisfy CS8618 for EF Core or serialization
        private Order()
        {
            Status = default!;
            Items = default!;
        }

        public Order(Guid customerId)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
            Status = OrderStatus.Draft;
        }

        public void AddItem(Guid productId, Guid productVariantId, string name, Money price, int quantity)
        {
            if (Status != OrderStatus.Draft)
                throw new OrderDomainException("Cannot modify a finalized order.");

            OrderItem? existingOrderItem = Items.FirstOrDefault(i => i.ProductVariantId == productVariantId);
            if (existingOrderItem != default)
            {
                existingOrderItem.IncreaseQuantity(quantity);
            }
            else
            {
                Items.Add(new OrderItem(productId, productVariantId, name, price, quantity));
            }

            RecalculateTotal();

            this.AddDomainEvent(new OrderCreatedDomainEvent(Id, productId, productVariantId, quantity));
        }

        public void MarkAsPaid()
        {
            if (Status != OrderStatus.Draft)
                throw new OrderDomainException("Order cannot be paid.");

            if (!Items.Any())
                throw new OrderDomainException("Cannot pay an empty order.");

            Status = OrderStatus.Paid;

            AddDomainEvent(new OrderPaidDomainEvent(Id));
        }

        public void SetShippingAddress(Address address)
        {
            if (Status != OrderStatus.Draft)
                throw new OrderDomainException("Cannot change address after payment.");

            ShippingAddress = address;
        }

        public void Cancel(string reason)
        {
            if (Status == OrderStatus.Shipped)
                throw new OrderDomainException("Shipped orders cannot be cancelled.");

            if (Status == OrderStatus.Cancelled)
                return;

            if (string.IsNullOrWhiteSpace(reason))
                throw new OrderDomainException("Cancellation reason is required.");

            CancellationReason = reason;
            Status = OrderStatus.Cancelled;

            AddDomainEvent(new OrderCancelledDomainEvent(Id, reason));
        }

        public void MarkAsShipped(string trackingNumber)
        {
            if (Status != OrderStatus.Paid)
                throw new OrderDomainException("Only paid orders can be shipped.");

            if (string.IsNullOrWhiteSpace(trackingNumber))
                throw new OrderDomainException("Tracking number is required.");

            TrackingNumber = trackingNumber;
            ShippedAt = DateTime.UtcNow;
            Status = OrderStatus.Shipped;

            AddDomainEvent(new OrderShippedDomainEvent(Id, trackingNumber));
        }

        public void RemoveItem(Guid itemId)
        {
            if (Status != OrderStatus.Draft)
                throw new OrderDomainException("Cannot modify a finalized order.");

            OrderItem? itemToRemove = Items.FirstOrDefault(i => i.Id == itemId);

            if (itemToRemove is null)
                throw new OrderDomainException("Item not found in the order.");

            Items.Remove(itemToRemove);

            RecalculateTotal();

            this.AddDomainEvent(new OrderItemRemovedDomainEvent(Id, itemToRemove.ProductId, itemToRemove.ProductVariantId));
        }

        private void RecalculateTotal()
        {
            TotalPrice = this.Items.Sum(i => i.TotalPrice);
        }
    }
}
