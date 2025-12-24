using MediatR;

namespace CatalogService.Application.Products.Queries
{
    public record GetProductVariantsQuery
        (Guid Id) : IRequest<IEnumerable<ProductVariantDto>>;
}