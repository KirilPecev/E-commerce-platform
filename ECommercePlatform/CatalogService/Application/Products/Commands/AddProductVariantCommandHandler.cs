using CatalogService.Application.Exceptions;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Commands
{
    public class AddProductVariantCommandHandler
        (CatalogDbContext dbContext) : IRequestHandler<AddProductVariantCommand, Guid>
    {
        public async Task<Guid> Handle(AddProductVariantCommand request, CancellationToken cancellationToken)
        {
            Product? product = await dbContext
                .Products
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
                throw new NotFoundException(nameof(Product), request.Id);

            Guid variantId = product.AddProductVariant(request.Sku, request.Amount, request.Currency, request.StockQuantity, request.Size, request.Color);

            await dbContext.SaveChangesAsync(cancellationToken);

            return variantId;
        }
    }
}