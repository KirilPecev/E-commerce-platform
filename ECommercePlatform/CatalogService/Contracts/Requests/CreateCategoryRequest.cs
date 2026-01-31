namespace CatalogService.Contracts.Requests
{
    public record CreateCategoryRequest(
        string Name,
        string? Description
        );
}
