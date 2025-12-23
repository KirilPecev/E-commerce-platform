
using MediatR;

namespace CatalogService.Domain.Events
{
    public class ProductCreatedDomainEvent : IDomainEvent, INotification
    {
        public Guid ProductId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ProductCreatedDomainEvent(Guid productId)
        {
            ProductId = productId;
        }
    }
}
