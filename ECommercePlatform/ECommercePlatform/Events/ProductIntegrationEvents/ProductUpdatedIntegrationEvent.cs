namespace ECommercePlatform.Events.ProductIntegrationEvents
{
    public class ProductUpdatedIntegrationEvent
    {
        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}