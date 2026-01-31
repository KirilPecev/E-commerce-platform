
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Categories.Queries
{
    public class GetCaterogyByIdQueryHandler
        (CatalogDbContext dbContext) : IRequestHandler<GetCaterogyByIdQuery, CategoryDto?>
    {
        public async Task<CategoryDto?> Handle(GetCaterogyByIdQuery request, CancellationToken cancellationToken)
        {
            CategoryDto? category = await dbContext
                .Categories
                .Where(c => c.Id == request.Id)
                .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
                .FirstOrDefaultAsync(cancellationToken);

            if (category is null)
            {
                throw new KeyNotFoundException($"Category with Id '{request.Id}' was not found.");
            }

            return category;
        }
    }
}
