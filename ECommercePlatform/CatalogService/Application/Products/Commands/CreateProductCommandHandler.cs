
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Commands
{
    public class CreateProductCommandHandler
        (CatalogDbContext dbContext,
        IProductCache cache) : IRequestHandler<CreateProductCommand, Guid>
    {
        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Category category = await dbContext
                .Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
                ?? throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found.");

            Product product = new Product(
                new ProductName(request.Name),
                new Money(request.Amount, request.Currency),
                category,
                request.Description);

            dbContext.Products.Add(product);

            await dbContext.SaveChangesAsync(cancellationToken);

            await cache.RemoveAllAsync();

            return product.Id;
        }
    }
}
