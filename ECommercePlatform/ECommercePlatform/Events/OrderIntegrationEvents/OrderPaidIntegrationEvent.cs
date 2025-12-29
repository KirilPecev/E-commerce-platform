namespace ECommercePlatform.Events.OrderIntegrationEvents
{
    public class OrderPaidIntegrationEvent
    {
        public Guid OrderId { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
