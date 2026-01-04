namespace ECommercePlatform.Events.InventoryIntegrationEvents
{
    public record StockReservedIntegrationEvent(
        Guid OrderId,
        Guid ProductId,
        Guid ProductVariantId,
        int Quantity);
}
