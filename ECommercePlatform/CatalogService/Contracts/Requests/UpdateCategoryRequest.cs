namespace CatalogService.Contracts.Requests
{
    public record UpdateCategoryRequest(
        string Name,
        string? Description
        );
}
