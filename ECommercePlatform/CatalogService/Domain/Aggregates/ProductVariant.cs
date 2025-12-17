using CatalogService.Domain.Common;
using CatalogService.Domain.ValueObjects;

namespace CatalogService.Domain.Aggregates
{
    public class ProductVariant : Entity
    {
        public string Sku { get; private set; }
        public Money Price { get; private set; }
        public string? Size { get; private set; }
        public string? Color { get; private set; }
        public int StockQuantity { get; private set; }

        public ProductVariant(Guid id, string sku, Money price, string? size, string? color, int stockQuantity)
        {
            Id = id;
            Sku = sku;
            Price = price;
            Size = size;
            Color = color;
            StockQuantity = stockQuantity;
        }
    }
}