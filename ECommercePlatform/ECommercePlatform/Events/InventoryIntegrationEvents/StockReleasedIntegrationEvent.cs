namespace ECommercePlatform.Events.InventoryIntegrationEvents
{
    public class StockReleasedIntegrationEvent()
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }
    }
}
