
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Queries
{
    public class GetProductVariantsQueryHandler
        (CatalogDbContext dbContext) : IRequestHandler<GetProductVariantsQuery, IEnumerable<ProductVariantDto>>
    {
        public async Task<IEnumerable<ProductVariantDto>> Handle(GetProductVariantsQuery request, CancellationToken cancellationToken)
            => await dbContext
                .ProductVariants
                .AsNoTracking()
                .Where(p => p.ProductId == request.Id)
                .Select(p => new ProductVariantDto(p.Id, p.Sku, p.Price.Amount, p.Price.Currency, p.Size, p.Color, p.StockQuantity))
                .ToListAsync(cancellationToken);
    }
}