
using ECommercePlatform.Domain.Events;

using MediatR;

namespace CatalogService.Domain.Events
{
    public class ProductCreatedDomainEvent : IDomainEvent, INotification
    {
        public Guid ProductId { get; }
        public Guid ProductVariantId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public int InitialQuantity { get; set; }

        public ProductCreatedDomainEvent(Guid productId, Guid productVariantId, int initialQuantity)
        {
            ProductId = productId;
            ProductVariantId = productVariantId;
            InitialQuantity = initialQuantity;
        }
    }
}