using CatalogService.Domain.Exceptions;
using CatalogService.Domain.ValueObjects;

using FluentAssertions;

namespace CatalogService.Tests.DomainUnitTests
{
    public class ProductNameTests
    {
        [Fact]
        public void Ctor_SetsValue_WhenValid()
        {
            var name = new ProductName("Laptop");

            name.Value.Should().Be("Laptop");
        }

        [Fact]
        public void Ctor_Throws_WhenEmpty()
        {
            Action act = () => new ProductName("");

            act.Should().Throw<CatalogDomainException>()
                .WithMessage("Product name is required.");
        }

        [Fact]
        public void Ctor_Throws_WhenNull()
        {
            Action act = () => new ProductName(null!);

            act.Should().Throw<CatalogDomainException>()
                .WithMessage("Product name is required.");
        }

        [Fact]
        public void Ctor_Throws_WhenWhitespace()
        {
            Action act = () => new ProductName("   ");

            act.Should().Throw<CatalogDomainException>()
                .WithMessage("Product name is required.");
        }

        [Fact]
        public void Ctor_Throws_WhenExceeds100Characters()
        {
            var longName = new string('A', 101);

            Action act = () => new ProductName(longName);

            act.Should().Throw<CatalogDomainException>()
                .WithMessage("Product name cannot exceed 100 characters.");
        }

        [Fact]
        public void Ctor_Succeeds_WhenExactly100Characters()
        {
            var name = new ProductName(new string('A', 100));

            name.Value.Length.Should().Be(100);
        }

        [Fact]
        public void Equality_ShouldBeEqual_WhenSameValue()
        {
            var name1 = new ProductName("Laptop");
            var name2 = new ProductName("Laptop");

            name1.Should().Be(name2);
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenDifferentValue()
        {
            var name1 = new ProductName("Laptop");
            var name2 = new ProductName("Desktop");

            name1.Should().NotBe(name2);
        }
    }
}
