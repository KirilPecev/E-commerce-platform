namespace CatalogService.Contracts.Responses
{
    public record ProductResponse(
        Guid Id,
        string Name,
        decimal Amount,
        string Currency,
        Guid CategoryId,
        string? Description);
}
