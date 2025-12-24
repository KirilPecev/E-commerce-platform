using CatalogService.Domain.Events;
using CatalogService.Domain.Exceptions;
using CatalogService.Domain.ValueObjects;

namespace CatalogService.Domain.Aggregates
{
    public class Product : AggregateRoot
    {
        public ProductName Name { get; private set; }
        public Money Price { get; private set; }
        public Guid CategoryId { get; private set; }
        public ProductStatus Status { get; private set; }
        public string? Description { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public List<ProductVariant> Variants { get; private set; } = new();

        public Product(ProductName name, Money price, Guid categoryId, string? description = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Price = price;
            CategoryId = categoryId;
            Status = ProductStatus.Active;
            Description = description;
            CreatedAt = DateTime.UtcNow;

            AddDomainEvent(new ProductCreatedDomainEvent(Id));
        }

        public Guid AddProductVariant(string sku, decimal amount, string currency, int stockQuantity, string? size, string? color)
        {
            if (Status == ProductStatus.Inactive)
                throw new CatalogDomainException("Cannot add variant to inactive product");

            ProductVariant productVariant = new ProductVariant(Id, sku, new Money(amount, currency), size, color, stockQuantity);

            Variants.Add(productVariant);

            AddDomainEvent(new ProductCreatedDomainEvent(Id));

            return productVariant.Id;
        }

        public void ChangePrice(Money newPrice)
        {
            if (Status == ProductStatus.Inactive)
                throw new CatalogDomainException("Cannot change price of inactive product");

            Price = newPrice;
        }

        public void UpdateDetails(ProductName name, Guid categoryId, string? description)
        {
            if (Status == ProductStatus.Inactive)
                throw new CatalogDomainException("Cannot update details of inactive product");

            Name = name;
            CategoryId = categoryId;
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
