namespace ECommercePlatform.Events.OrderIntegrationEvents
{
    public class OrderCreatedIntegrationEvent
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
