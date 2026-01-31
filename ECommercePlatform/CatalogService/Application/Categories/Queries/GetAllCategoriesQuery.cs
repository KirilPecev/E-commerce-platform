using MediatR;

namespace CatalogService.Application.Categories.Queries
{
    public record GetAllCategoriesQuery(): IRequest<IEnumerable<CategoryDto>>;
}
