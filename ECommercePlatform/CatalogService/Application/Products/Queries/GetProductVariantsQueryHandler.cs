
using CatalogService.Application.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Queries
{
    public class GetProductVariantsQueryHandler
        (ICatalogDbContext dbContext) : IRequestHandler<GetProductVariantsQuery, IEnumerable<ProductVariantDto>>
    {
        public async Task<IEnumerable<ProductVariantDto>> Handle(GetProductVariantsQuery request, CancellationToken cancellationToken)
            => await dbContext
                .ProductVariants
                .AsNoTracking()
                .Where(p => p.Product.Id == request.Id)
                .Select(p => new ProductVariantDto(p.Id, p.Sku, p.Price.Amount, p.Price.Currency, p.Size, p.Color, p.StockQuantity))
                .ToListAsync(cancellationToken);
    }
}