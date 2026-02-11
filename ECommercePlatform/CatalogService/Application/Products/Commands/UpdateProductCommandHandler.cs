using CatalogService.Application.Exceptions;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Commands
{
    public class UpdateProductCommandHandler
        (CatalogDbContext dbContext,
        IProductCache cache) : IRequestHandler<UpdateProductCommand>
    {
        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            Product product = await dbContext
                .Products
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), request.Id);

            Category category = await dbContext
                .Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(Category), request.Id);

            product.UpdateDetails(new ProductName(request.Name), category, request.Description);
            product.ChangePrice(new Money(request.Amount, request.Currency));

            await dbContext.SaveChangesAsync(cancellationToken);

            await cache.RemoveByIdAsync(product.Id);
            await cache.RemoveAllAsync();
        }
    }
}
