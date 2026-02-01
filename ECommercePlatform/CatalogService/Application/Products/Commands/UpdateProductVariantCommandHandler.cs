using CatalogService.Application.Exceptions;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.ValueObjects;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Commands
{
    public class UpdateProductVariantCommandHandler
        (CatalogDbContext dbContext) : IRequestHandler<UpdateProductVariantCommand>
    {
        public async Task Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
        {
            ProductVariant? productVariant = await dbContext
                .ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(p => p.Id == request.VariantId
                                       && p.Product.Id == request.ProductId, cancellationToken);

            if (productVariant is null)
                throw new NotFoundException(nameof(Product), request.VariantId);

            productVariant.UpdateDetails(request.Sku, request.Size, request.Color, request.StockQuantity);
            productVariant.ChangePrice(new Money(request.Amount, request.Currency));

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
