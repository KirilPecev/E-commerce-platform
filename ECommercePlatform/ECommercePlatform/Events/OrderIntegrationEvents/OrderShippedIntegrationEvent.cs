namespace ECommercePlatform.Events.OrderIntegrationEvents
{
    public class OrderShippedIntegrationEvent
    {
        public Guid OrderId { get; set; }
        public string TrackingNumber { get; set; } = default!;
        public DateTime OccurredOn { get; set; }
    }
}
