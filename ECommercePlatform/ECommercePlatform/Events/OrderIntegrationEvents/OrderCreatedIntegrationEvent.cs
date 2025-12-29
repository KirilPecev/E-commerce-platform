namespace ECommercePlatform.Events.OrderIntegrationEvents
{
    public class OrderCreatedIntegrationEvent
    {
        public Guid OrderId { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
