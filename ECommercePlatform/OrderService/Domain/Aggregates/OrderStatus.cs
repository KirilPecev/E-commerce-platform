namespace OrderService.Domain.Aggregates
{
    public enum OrderStatus
    {
        Draft,
        Finalized,
        Paid,
        Shipped,
        Cancelled
    }
}
