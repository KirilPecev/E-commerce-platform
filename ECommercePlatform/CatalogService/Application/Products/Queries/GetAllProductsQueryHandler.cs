
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Queries
{
    public class GetAllProductsQueryHandler
        (CatalogDbContext dbContext,
        IProductCache cache) : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            IReadOnlyList<ProductDto>? cached = await cache.GetAllAsync();

            if (cached != null) return cached;

            List<ProductDto> products = await dbContext
                .Products
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Active)
                .Select(product => new ProductDto(
                    product.Id,
                    product.Name.Value,
                    product.Price.Amount,
                    product.Price.Currency,
                    product.Category.Id,
                    product.Description
                ))
                .ToListAsync(cancellationToken);

            await cache.SetAllAsync(products);

            return products;
        }
    }
}
