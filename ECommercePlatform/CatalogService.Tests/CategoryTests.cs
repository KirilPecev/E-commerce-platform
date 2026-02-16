using CatalogService.Domain.Aggregates;
using CatalogService.Domain.Exceptions;
using CatalogService.Domain.ValueObjects;

using FluentAssertions;

namespace CatalogService.Tests
{
    public class CategoryTests
    {
        [Fact]
        public void Create_ShouldCreateValidCategory()
        {
            // Act
            Category category = new Category("Electronics", "All kinds of electronic devices");

            // Assert
            category.Id.Should().NotBeEmpty();
            category.Name.Should().Be("Electronics");
            category.Description.Should().Be("All kinds of electronic devices");
        }

        [Fact]
        public void Create_ShouldCreateValidCategoryWithId()
        {
            // Act
            Category category = new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "All kinds of electronic devices");

            // Assert
            category.Id.Should().Be(Guid.Parse("11111111-0000-0000-0000-000000000001"));
            category.Name.Should().Be("Electronics");
            category.Description.Should().Be("All kinds of electronic devices");
        }

        [Fact]
        public void Update_ShouldChangeCategoryData()
        {
            // Act
            Category category = new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "All kinds of electronic devices");

            category.UpdateDetails("Home Appliances", "All kinds of home appliances");

            // Assert
            category.Id.Should().NotBeEmpty();
            category.Name.Should().Be("Home Appliances");
            category.Description.Should().Be("All kinds of home appliances");
        }

        [Fact]
        public void Create_ShouldThrow_WhenNameIsEmpty()
        {
            // Act
            Action act = () => new Category(
                "",
                "All kinds of electronic devices");

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Create_ShouldThrow_WhenNameLengthInvalid()
        {
            // Act
            Action act = () => new Category(
                "All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices",
                "All kinds of electronic devices");

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Create_ShouldThrow_WhenNameDescriptionInvalid()
        {
            // Act
            Action act = () => new Category(
                "Electronics",
                "All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices ,All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices");

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Create_ShouldThrow_WhenNameIsEmptyWithId()
        {
            // Act
            Action act = () => new Category(
                Guid.Parse("11111111-0000-0000-0000-000000000001"),
                "",
                "All kinds of electronic devices");

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Create_ShouldThrow_WhenNameLengthInvalidWithId()
        {
            // Act
            Action act = () => new Category(
                Guid.Parse("11111111-0000-0000-0000-000000000001"),
                "All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices",
                "All kinds of electronic devices");

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Create_ShouldThrow_WhenNameDescriptionInvalidWithId()
        {
            // Act
            Action act = () => new Category(
                Guid.Parse("11111111-0000-0000-0000-000000000001"),
                "Electronics",
                "All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices ,All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices");

            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Update_ShouldThrow_WhenNameIsEmpty()
        {
            // Act
            Category category = new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "All kinds of electronic devices");
                        
            Action act = () =>  category.UpdateDetails("", "All kinds of home appliances");

            // Assert
            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Update_ShouldThrow_WhenNameLengthInvalidWithId()
        {
            // Act
            Category category = new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "All kinds of electronic devices");

            Action act = () => category.UpdateDetails("All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices", "All kinds of home appliances");

            // Assert
            act.Should().Throw<CatalogDomainException>();
        }

        [Fact]
        public void Update_ShouldThrow_WhenNameDescriptionInvalidWithId()
        {
            // Act
            Category category = new Category(Guid.Parse("11111111-0000-0000-0000-000000000001"), "Electronics", "All kinds of electronic devices");

            Action act = () => category.UpdateDetails("Electronics", "All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices ,All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices, All kinds of electronic devices");

            // Assert
            act.Should().Throw<CatalogDomainException>();
        }
    }
}
