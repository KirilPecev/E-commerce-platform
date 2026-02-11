using CatalogService.Application.Products.Queries;

namespace CatalogService.Application.Interfaces
{
    public interface IProductCache
    {
        Task<ProductDto?> GetByIdAsync(Guid productId);

        Task<IReadOnlyList<ProductDto>?> GetAllAsync();

        Task SetByIdAsync(ProductDto product);

        Task SetAllAsync(IReadOnlyList<ProductDto> products);

        Task RemoveByIdAsync(Guid productId);

        Task RemoveAllAsync();
    }
}
