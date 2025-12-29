namespace ECommercePlatform.Events.ProductIntegrationEvents
{
    public class ProductUpdatedIntegrationEvent
    {
        public Guid ProductId { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}