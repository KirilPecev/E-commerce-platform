
using CatalogService.Application.Exceptions;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Commands
{
    public class AddProductVariantCommandHandler
        (CatalogDbContext dbContext) : IRequestHandler<AddProductVariantCommand>
    {
        public async Task Handle(AddProductVariantCommand request, CancellationToken cancellationToken)
        {
            Product? product = await dbContext
                .Products
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
                throw new NotFoundException(nameof(Product), request.Id);
        }
    }
}