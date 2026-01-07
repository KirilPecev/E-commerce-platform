namespace InventoryService.Contracts.Responses
{
    public record ProductStocksResponse(
        Guid ProductId,
        Guid ProductVariantId,
        int AvailableQuantity,
        int ReservedQuantity);
}
