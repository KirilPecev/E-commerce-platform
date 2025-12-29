namespace ECommercePlatform.Events.OrderIntegrationEvents
{
    public class OrderCancelledIntegrationEvent
    {
        public Guid OrderId { get; set; }
        public string Reason { get; set; } = default!;
        public DateTime OccurredOn { get; set; }
    }
}
