namespace CatalogService.Contracts.Requests
{
    public record UpdateProductRequest(
        string Name,
        decimal Amount,
        string Currency,
        Guid CategoryId,
        string? Description);
}
