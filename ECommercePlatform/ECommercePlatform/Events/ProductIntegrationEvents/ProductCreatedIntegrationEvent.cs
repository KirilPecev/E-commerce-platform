namespace ECommercePlatform.Events.ProductIntegrationEvents
{
    public class ProductCreatedIntegrationEvent
    {
        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }
        public DateTime OccurredOn { get; set; }
        public int InitialQuantity { get; set; }
    }
}
