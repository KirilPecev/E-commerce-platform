using MediatR;

namespace CatalogService.Application.Categories.Commands
{
    public record DeleteCategoryCommand(
        Guid Id
        ) : IRequest;
}
