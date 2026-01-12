namespace ECommercePlatform.Events.InventoryIntegrationEvents
{
    public class StockReservationFailedIntegrationEvent
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }
        public string Reason { get; set; } = default!;
    }
}
