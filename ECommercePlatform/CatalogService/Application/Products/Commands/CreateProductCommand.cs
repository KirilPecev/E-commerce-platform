using MediatR;

namespace CatalogService.Application.Products.Commands
{
    public record CreateProductCommand(
        string Name,
        decimal Price) : IRequest<Guid>;
}
