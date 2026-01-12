namespace ECommercePlatform.Events.PaymentIntegrationEvents
{
    public class PaymentCompletedIntegrationEvent
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
