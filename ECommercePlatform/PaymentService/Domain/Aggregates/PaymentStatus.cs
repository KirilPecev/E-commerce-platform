namespace PaymentService.Domain.Aggregates
{
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }
}
