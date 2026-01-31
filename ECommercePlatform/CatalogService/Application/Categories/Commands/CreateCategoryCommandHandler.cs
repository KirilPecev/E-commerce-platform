
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Categories.Commands
{
    public class CreateCategoryCommandHandler
        (CatalogDbContext dbContext) : IRequestHandler<CreateCategoryCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            bool alreadyExists = await dbContext
                .Categories
                .AnyAsync(c => c.Name == request.Name, cancellationToken);

            if (alreadyExists)
            {
                throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");
            }

            Category category = new Category(request.Name, request.Description);

            dbContext.Categories.Add(category);

            await dbContext.SaveChangesAsync(cancellationToken);

            return category.Id;
        }
    }
}
