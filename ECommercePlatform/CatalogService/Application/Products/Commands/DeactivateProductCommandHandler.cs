using CatalogService.Application.Exceptions;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Commands
{
    public class DeactivateProductCommandHandler
        (CatalogDbContext dbContext,
        IProductCache cache) : IRequestHandler<DeactivateProductCommand>
    {
        public async Task Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
        {
            Product? product = await dbContext
                .Products
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
                throw new NotFoundException(nameof(Product), request.Id);

            product.Deactivate();

            await dbContext.SaveChangesAsync(cancellationToken);

            await cache.RemoveByIdAsync(product.Id);
            await cache.RemoveAllAsync();
        }
    }
}
