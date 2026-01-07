
namespace InventoryService.Application.Inventory.Queries
{
    public record ProductStockDto(
        Guid ProductId,
        Guid ProductVariantId,
        int QuantityAvailable,
        int QuantityReserved);
}
