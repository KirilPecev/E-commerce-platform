
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Queries
{
    public class GetProductByIdQueryHandler
        (CatalogDbContext dbContext) : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
            => await dbContext
                .Products
                .AsNoTracking()
                .Where(p => p.Id == request.Id && p.Status == ProductStatus.Active)
                .Select(p => new ProductDto(p.Id, p.Name.Value, p.Price.Amount, p.Price.Currency, p.Category.Id, p.Description))
                .FirstOrDefaultAsync(cancellationToken);
    }
}
