using ECommercePlatform.Domain;

using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Aggregates
{
    public class OrderItem : Entity
    {
        public Guid ProductId { get; private set; }
        public Guid ProductVariantId { get; private set; }
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public Money UnitPrice { get; private set; }
        public decimal TotalPrice => UnitPrice.Amount * Quantity;

        // Initialize non-nullable properties with default! to satisfy CS8618 for EF Core or serialization
        private OrderItem()
        {
            ProductVariantId = default!;
            ProductName = default!;
            UnitPrice = default!;
        }

        public OrderItem(Guid productId, Guid productVariantId, string productName, Money unitPrice, int quantity)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            ProductName = productName;
            ProductVariantId = productVariantId;
            UnitPrice = unitPrice;
            Quantity = quantity;

            if (quantity <= 0)
                throw new OrderDomainException("Quantity must be greater than zero");
        }

        public void IncreaseQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new OrderDomainException("Quantity must be greater than zero");

            Quantity += quantity;
        }
    }
}
