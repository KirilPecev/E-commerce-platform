using MediatR;

namespace CatalogService.Application.Products.Queries
{
    public record GetProductVariantByIdQuery
        (Guid ProductId, Guid VariantId) : IRequest<ProductVariantDto?>;
}