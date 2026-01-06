
using ECommercePlatform.Domain.Events;

using MediatR;

namespace OrderService.Domain.Events
{
    public class OrderCreatedDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public Guid ProductId { get; }
        public Guid ProductVariantId { get; }
        public int Quantity { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public OrderCreatedDomainEvent(Guid orderId, Guid productId, Guid productVariantId, int quantity)
        {
            OrderId = orderId;
            ProductId = productId;
            ProductVariantId = productVariantId;
            Quantity = quantity;
        }
    }
}
