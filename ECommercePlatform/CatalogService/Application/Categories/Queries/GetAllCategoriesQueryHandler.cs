using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Categories.Queries
{
    public class GetAllCategoriesQueryHandler
        (CatalogDbContext dbContext) : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
    {
        public async Task<IEnumerable<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            List<CategoryDto> categories = await dbContext
                .Categories
                .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
                .ToListAsync(cancellationToken);

            return categories;
        }
    }
}
