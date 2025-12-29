namespace ECommercePlatform.Events.ProductIntegrationEvents
{
    public class ProductCreatedIntegrationEvent
    {
        public Guid ProductId { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
