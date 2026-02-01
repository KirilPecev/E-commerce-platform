namespace CatalogService.Contracts.Requests
{
    public record AddProductVariantRequest(
        string Sku,
        decimal Amount,
        string Currency,
        int StockQuantity,
        string? Size,
        string? Color
        );
}