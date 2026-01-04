using CatalogService.Domain.Events;
using CatalogService.Domain.Exceptions;
using CatalogService.Domain.ValueObjects;

using ECommercePlatform.Domain.Abstractions;

namespace CatalogService.Domain.Aggregates
{
    public class Product : AggregateRoot
    {
        public ProductName Name { get; private set; }
        public Money Price { get; private set; }
        public Category Category { get; private set; }
        public ProductStatus Status { get; private set; }
        public string? Description { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public List<ProductVariant> Variants { get; private set; } = new();

        // Initialize non-nullable properties with default! to satisfy CS8618 for EF Core or serialization
        private Product()
        {
            Name = default!;
            Price = default!;
            Category = default!;
        }

        public Product(ProductName name, Money price, Category category, string? description = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Price = price;
            Category = category;
            Status = ProductStatus.Active;
            Description = description;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid AddProductVariant(string sku, decimal amount, string currency, int stockQuantity, string? size, string? color)
        {
            if (Status == ProductStatus.Inactive)
                throw new CatalogDomainException("Cannot add variant to inactive product");

            ProductVariant productVariant = new ProductVariant(this, sku, new Money(amount, currency), size, color, stockQuantity);

            Variants.Add(productVariant);

            AddDomainEvent(new ProductCreatedDomainEvent(Id, productVariant.Id, stockQuantity));

            return productVariant.Id;
        }

        public void ChangePrice(Money newPrice)
        {
            if (Status == ProductStatus.Inactive)
                throw new CatalogDomainException("Cannot change price of inactive product");

            Price = newPrice;
        }

        public void UpdateDetails(ProductName name, Category category, string? description)
        {
            if (Status == ProductStatus.Inactive)
                throw new CatalogDomainException("Cannot update details of inactive product");

            Name = name;
            Category = category;
            Description = description;

            AddDomainEvent(new ProductUpdatedDomainEvent(Id));
        }

        public void Deactivate()
        {
            if (Status == ProductStatus.Inactive) return;

            Status = ProductStatus.Inactive;
        }

        public void Activate()
        {
            if (Status == ProductStatus.Active) return;

            Status = ProductStatus.Active;
        }
    }
}
