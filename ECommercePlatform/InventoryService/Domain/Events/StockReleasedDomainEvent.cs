
using ECommercePlatform.Domain.Events;

using MediatR;

namespace InventoryService.Domain.Events
{
    public class StockReleasedDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public Guid ProductId { get; }
        public Guid ProductVariantId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public StockReleasedDomainEvent(
            Guid orderId,
            Guid productId,
            Guid productVariantId)
        {
            OrderId = orderId;
            ProductId = productId;
            ProductVariantId = productVariantId;
        }
    }
}
