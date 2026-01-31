using MediatR;

namespace CatalogService.Application.Categories.Commands
{
    public record UpdateCategoryCommand(
        Guid Id,
        string Name,
        string? Description
        ) : IRequest<Guid>;
}
