using MediatR;

namespace CatalogService.Application.Categories.Queries
{
    public record GetCaterogyByIdQuery(
        Guid Id
        ) : IRequest<CategoryDto?>;
}
