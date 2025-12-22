using CatalogService.Domain.Common;
using CatalogService.Domain.Exceptions;

namespace CatalogService.Domain.Aggregates
{
    public class Category : Entity
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public List<Product> Products { get; private set; } = new();

        public Category(Guid id, string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new CatalogDomainException("Category name is required.");

            if (name.Length > 100)
                throw new CatalogDomainException("Category name cannot exceed 100 characters.");

            if (description != default && description.Length > 500)
                throw new CatalogDomainException("Description cannot exceed 500 characters.");

            Id = id;
            Name = name;
            Description = description;
        }
    }
}
