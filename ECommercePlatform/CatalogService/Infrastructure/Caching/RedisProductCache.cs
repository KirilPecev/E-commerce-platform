using System.Text.Json;

using CatalogService.Application.Interfaces;
using CatalogService.Application.Products.Queries;

using Microsoft.Extensions.Caching.Distributed;

namespace CatalogService.Infrastructure.Caching
{
    public class RedisProductCache : IProductCache
    {
        private readonly IDistributedCache cache;

        private static readonly DistributedCacheEntryOptions CacheOptions =
            new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };

        private static string ProductKey(Guid id) => $"product:{id}";

        private const string AllProductsKey = "products:all";

        public RedisProductCache(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public async Task<ProductDto?> GetByIdAsync(Guid productId)
        {
            string? json = await this.cache.GetStringAsync(ProductKey(productId));

            return json is null
                ? null
                : JsonSerializer.Deserialize<ProductDto>(json);
        }

        public async Task<IReadOnlyList<ProductDto>?> GetAllAsync()
        {
            string? json = await this.cache.GetStringAsync(AllProductsKey);

            return json is null
                ? null
                : JsonSerializer.Deserialize<IReadOnlyList<ProductDto>>(json);
        }

        public async Task SetByIdAsync(ProductDto product)
        {
            await this.cache.SetStringAsync(
                ProductKey(product.Id),
                JsonSerializer.Serialize(product),
                CacheOptions);
        }

        public async Task SetAllAsync(IReadOnlyList<ProductDto> products)
        {
            await this.cache.SetStringAsync(
                AllProductsKey,
                JsonSerializer.Serialize(products),
                CacheOptions);
        }

        public async Task RemoveByIdAsync(Guid productId)
        {
            await this.cache.RemoveAsync(ProductKey(productId));
        }

        public async Task RemoveAllAsync()
        {
            await this.cache.RemoveAsync(AllProductsKey);
        }
    }
}
