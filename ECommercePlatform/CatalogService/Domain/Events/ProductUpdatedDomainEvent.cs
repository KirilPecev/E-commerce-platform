
using ECommercePlatform.Domain.Events;

using MediatR;

namespace CatalogService.Domain.Events
{
    public class ProductUpdatedDomainEvent : IDomainEvent, INotification
    {
        public Guid ProductId { get; }
        public Guid ProductVariantId { get; }
        public int Quantity { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ProductUpdatedDomainEvent(Guid productId, Guid productVariantId, int quantity)
        {
            ProductId = productId;
            ProductVariantId = productVariantId;
            Quantity = quantity;
        }
    }
}
