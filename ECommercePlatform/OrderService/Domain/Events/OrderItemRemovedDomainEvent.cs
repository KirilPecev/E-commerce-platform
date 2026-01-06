using ECommercePlatform.Domain.Events;

using MediatR;

namespace OrderService.Domain.Events
{
    public class OrderItemRemovedDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public Guid ProductId { get; }
        public Guid ProductVariantId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public OrderItemRemovedDomainEvent(Guid orderId, Guid productId, Guid productVariantId)
        {
            OrderId = orderId;
            ProductId = productId;
            ProductVariantId = productVariantId;
        }
    }
}
