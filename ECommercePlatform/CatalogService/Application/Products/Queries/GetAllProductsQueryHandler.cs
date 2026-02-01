
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;

using MassTransit.DependencyInjection;
using MassTransit.Internals.GraphValidation;

using MediatR;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Queries
{
    public class GetAllProductsQueryHandler
        (CatalogDbContext dbContext) : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
            => await dbContext
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
    }
}
