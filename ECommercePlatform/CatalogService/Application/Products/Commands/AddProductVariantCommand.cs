
using MediatR;

namespace CatalogService.Application.Products.Commands
{
    public record AddProductVariantCommand(Guid Id) : IRequest;
}