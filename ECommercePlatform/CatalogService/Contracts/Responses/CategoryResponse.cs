namespace CatalogService.Contracts.Responses
{
    public record CategoryResponse(
        Guid Id,
        string Name,
        string? Description
        );
}
