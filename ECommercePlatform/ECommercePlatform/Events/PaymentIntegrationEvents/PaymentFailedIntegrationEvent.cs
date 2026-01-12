namespace ECommercePlatform.Events.PaymentIntegrationEvents
{
    public class PaymentFailedIntegrationEvent
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public string? FailureReason { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}