
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Queries
{
    public class GetProductVariantByIdQueryHandler
        (CatalogDbContext dbContext) : IRequestHandler<GetProductVariantByIdQuery, ProductVariantDto?>
    {
        public async Task<ProductVariantDto?> Handle(GetProductVariantByIdQuery request, CancellationToken cancellationToken)
            => await dbContext
                .ProductVariants
                .AsNoTracking()
                .Where(p => p.Id == request.VariantId
                         && p.ProductId == request.ProductId)
                .Select(p => new ProductVariantDto(p.Id, p.Sku, p.Price.Amount, p.Price.Currency, p.Size, p.Color, p.StockQuantity))
                .FirstOrDefaultAsync(cancellationToken);
    }
}