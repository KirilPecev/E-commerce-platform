namespace CatalogService.Application.Products.Queries
{
    public record ProductDto(
        Guid Id,
        string Name,
        decimal Amount,
        string Currency);
}
