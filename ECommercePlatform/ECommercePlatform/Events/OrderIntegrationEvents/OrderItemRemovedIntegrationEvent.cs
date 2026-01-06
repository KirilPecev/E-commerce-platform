namespace ECommercePlatform.Events.OrderIntegrationEvents
{
    public class OrderItemRemovedIntegrationEvent
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
