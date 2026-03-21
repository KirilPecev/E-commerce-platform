
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Aggregates;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Categories.Commands
{
    public class DeleteCategoryCommandHandler
        (ICatalogDbContext dbContext) : IRequestHandler<DeleteCategoryCommand>
    {
        public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            Category? category = await dbContext
                .Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with Id {request.Id} was not found.");
            }

            dbContext.Categories.Remove(category);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
