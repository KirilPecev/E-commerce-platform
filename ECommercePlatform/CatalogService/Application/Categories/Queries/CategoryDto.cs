namespace CatalogService.Application.Categories.Queries
{
    public record CategoryDto(
        Guid Id,
        string Name,
        string? Description
        );
}
