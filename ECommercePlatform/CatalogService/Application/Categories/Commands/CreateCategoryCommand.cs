using MediatR;

namespace CatalogService.Application.Categories.Commands
{
    public record CreateCategoryCommand(
        string Name,
        string? Description
        ) : IRequest<Guid>;
}
