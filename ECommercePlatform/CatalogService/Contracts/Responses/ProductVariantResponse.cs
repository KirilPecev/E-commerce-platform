namespace CatalogService.Contracts.Responses
{
    public record ProductVariantResponse
        (Guid Id,
        string Sku,
        decimal Amount,
        string Currency,
        string? Size,
        string? Color,
        int StockQuantity);
}