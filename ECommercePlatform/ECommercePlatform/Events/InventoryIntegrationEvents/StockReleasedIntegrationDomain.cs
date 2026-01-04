namespace ECommercePlatform.Events.InventoryIntegrationEvents
{
    public record StockReleasedIntegrationDomain(
        Guid OrderId,
        Guid ProductId,
        Guid ProductVariantId);
}
