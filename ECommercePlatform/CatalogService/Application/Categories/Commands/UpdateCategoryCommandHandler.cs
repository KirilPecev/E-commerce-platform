
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Categories.Commands
{
    public class UpdateCategoryCommandHandler
        (CatalogDbContext dbContext) : IRequestHandler<UpdateCategoryCommand, Guid>
    {
        public async Task<Guid> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            Category? category = await dbContext
                .Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with Id {request.Id} was not found.");
            }

            category.UpdateDetails(request.Name, request.Description);

            await dbContext.SaveChangesAsync(cancellationToken);

            return category.Id;
        }
    }
}
