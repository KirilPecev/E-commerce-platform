namespace ECommercePlatform.Events.InventoryIntegrationEvents
{
    public record StockReservationFailedIntegrationEvent(
        Guid OrderId,
        Guid ProductId,
        Guid ProductVariantId,
        string Reason);
}
