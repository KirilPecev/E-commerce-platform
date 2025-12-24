
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;
using CatalogService.Infrastructure.Persistence;

using MediatR;

namespace CatalogService.Application.Products.Commands
{
    public class CreateProductCommandHandler
        (CatalogDbContext dbContext) : IRequestHandler<CreateProductCommand, Guid>
    {
        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            Product product = new Product(
                new ProductName(request.Name),
                new Money(request.Amount, request.Currency),
                request.CategoryId,
                request.Description);

            dbContext.Products.Add(product);

            await dbContext.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
