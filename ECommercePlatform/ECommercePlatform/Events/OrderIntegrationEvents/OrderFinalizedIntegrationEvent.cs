namespace ECommercePlatform.Events.OrderIntegrationEvents
{
    public class OrderFinalizedIntegrationEvent
    {
        public Guid OrderId { get; set; }
        public DateTime OccurredOn { get; set; }
        public decimal Amount { get; set; }
        public string Currecy { get; set; } = default!;
    }
}
