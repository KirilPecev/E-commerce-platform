using CatalogService.Domain.Common;
using CatalogService.Domain.Exceptions;

namespace CatalogService.Domain.ValueObjects
{
    public class ProductName : ValueObject
    {
        public string Value { get; }

        public ProductName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new CatalogDomainException("Product name is required.");

            if (value.Length > 100)
                throw new CatalogDomainException("Product name cannot exceed 100 characters.");

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
