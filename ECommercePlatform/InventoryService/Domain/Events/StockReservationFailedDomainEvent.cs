
using ECommercePlatform.Domain.Events;

using MediatR;

namespace InventoryService.Domain.Events
{
    public class StockReservationFailedDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public Guid ProductId { get; }
        public Guid ProductVariantId { get; }
        public string Reason { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;


        public StockReservationFailedDomainEvent(
            Guid orderId,
            Guid productId,
            Guid productVariantId,
            string reason)
        {
            OrderId = orderId;
            ProductId = productId;
            ProductVariantId = productVariantId;
            Reason = reason;
        }
    }
}
