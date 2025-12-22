using CatalogService.Domain.Aggregates;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository
        (CatalogDbContext dbContext)
        : IProductRepository
    {
        public async Task AddAsync(Product product)
        {
            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid productId)
        {
            await dbContext
                .Products
                .Where(p => p.Id == productId)
                .ExecuteDeleteAsync();
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await dbContext.Products.ToArrayAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid productId)
        {
            return await dbContext.Products.FindAsync(productId);
        }

        public async Task UpdateAsync(Product product)
        {
            await dbContext
                .Products
                .Where(p => p.Id == product.Id)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(p => p.Name, product.Name)
                    .SetProperty(p => p.Price, product.Price)
                    .SetProperty(p => p.CategoryId, product.CategoryId)
                    .SetProperty(p => p.Status, product.Status)
                    .SetProperty(p => p.Description, product.Description)
                );
        }
    }
}
