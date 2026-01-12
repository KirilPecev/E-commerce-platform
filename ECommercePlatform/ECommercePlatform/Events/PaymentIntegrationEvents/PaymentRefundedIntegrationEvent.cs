namespace ECommercePlatform.Events.PaymentIntegrationEvents
{
    public class PaymentRefundedIntegrationEvent
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
