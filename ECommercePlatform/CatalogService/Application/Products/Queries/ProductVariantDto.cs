namespace CatalogService.Application.Products.Queries
{
    public record ProductVariantDto
        (Guid Id,
        string Sku,
        decimal Amount,
        string Currency,
        string? Size,
        string? Color,
        int StockQuantity);
}