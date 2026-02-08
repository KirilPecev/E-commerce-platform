
using ECommercePlatform.Domain.Events;

using MediatR;

namespace OrderService.Domain.Events
{
    public class OrderFinalizedDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public decimal TotalPrice { get; }
        public string Currency { get; }

        public OrderFinalizedDomainEvent(Guid orderId, decimal totalPrice, string currency)
        {
            OrderId = orderId;
            TotalPrice = totalPrice;
            Currency = currency;
        }
    }
}
