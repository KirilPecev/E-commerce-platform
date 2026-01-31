using CatalogService.Domain.Exceptions;

using ECommercePlatform.Domain;

namespace CatalogService.Domain.Aggregates
{
    public class Category : Entity
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public List<Product> Products { get; private set; } = new();

        // Initialize non-nullable properties with default! to satisfy CS8618 for EF Core or serialization
        private Category()
        {
            Name = default!;
        }

        public Category(Guid id, string name, string? description)
        {
            Validate(name, description);

            Id = id;
            Name = name;
            Description = description;
        }

        public Category(string name, string? description)
        {
            Validate(name, description);

            Id = Guid.NewGuid();
            Name = name;
            Description = description;
        }

        public void UpdateDetails(string name, string? description)
        {
            Validate(name, description);

            Name = name;
            Description = description;
        }

        private static void Validate(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new CatalogDomainException("Category name is required.");

            if (name.Length > 100)
                throw new CatalogDomainException("Category name cannot exceed 100 characters.");

            if (description != default && description.Length > 500)
                throw new CatalogDomainException("Description cannot exceed 500 characters.");
        }
    }
}
