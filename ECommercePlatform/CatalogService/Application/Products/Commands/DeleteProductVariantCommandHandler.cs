using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Commands
{
    public class DeleteProductVariantCommandHandler
        (CatalogDbContext dbContext) : IRequestHandler<DeleteProductVariantCommand>
    {
        public async Task Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
        {
            if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                var variants = await dbContext.ProductVariants
                    .Where(p => p.Id == request.VariantId &&
                                p.Product.Id == request.ProductId)
                    .ToListAsync(cancellationToken);

                dbContext.ProductVariants.RemoveRange(variants);

                await dbContext.SaveChangesAsync(cancellationToken);

                return;
            }

            await dbContext.ProductVariants
                .Where(p => p.Id == request.VariantId &&
                            p.Product.Id == request.ProductId)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
