using CatalogService.Domain.Common;
using CatalogService.Domain.Exceptions;

namespace CatalogService.Domain.Aggregates
{
    public class Category : Entity
    {
        public string Name { get; private set; }

        public Category(Guid id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new CatalogDomainException("Category name is required.");

            if (name.Length > 100)
                throw new CatalogDomainException("Category name cannot exceed 100 characters.");

            Id = id;
            Name = name;
        }
    }
}
