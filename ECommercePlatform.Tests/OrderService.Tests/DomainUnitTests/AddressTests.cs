using FluentAssertions;

using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Tests.DomainUnitTests
{
    public class AddressTests
    {
        [Fact]
        public void Ctor_SetsProperties_WhenValid()
        {
            var address = new Address("123 Main St", "Springfield", "62701", "US");

            address.Street.Should().Be("123 Main St");
            address.City.Should().Be("Springfield");
            address.ZipCode.Should().Be("62701");
            address.Country.Should().Be("US");
        }

        [Fact]
        public void Ctor_Throws_WhenStreetIsEmpty()
        {
            Action act = () => new Address("", "City", "12345", "US");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("Street is required.");
        }

        [Fact]
        public void Ctor_Throws_WhenStreetIsNull()
        {
            Action act = () => new Address(null!, "City", "12345", "US");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("Street is required.");
        }

        [Fact]
        public void Ctor_Throws_WhenCityIsEmpty()
        {
            Action act = () => new Address("Street", "", "12345", "US");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("City is required.");
        }

        [Fact]
        public void Ctor_Throws_WhenCityIsNull()
        {
            Action act = () => new Address("Street", null!, "12345", "US");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("City is required.");
        }

        [Fact]
        public void Ctor_Throws_WhenZipCodeIsEmpty()
        {
            Action act = () => new Address("Street", "City", "", "US");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("ZipCode is required.");
        }

        [Fact]
        public void Ctor_Throws_WhenZipCodeIsNull()
        {
            Action act = () => new Address("Street", "City", null!, "US");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("ZipCode is required.");
        }

        [Fact]
        public void Ctor_Throws_WhenCountryIsEmpty()
        {
            Action act = () => new Address("Street", "City", "12345", "");

            act.Should().Throw<OrderDomainException>()
                .WithMessage("Country is required.");
        }

        [Fact]
        public void Ctor_Throws_WhenCountryIsNull()
        {
            Action act = () => new Address("Street", "City", "12345", null!);

            act.Should().Throw<OrderDomainException>()
                .WithMessage("Country is required.");
        }

        [Fact]
        public void Equality_ShouldBeEqual_WhenSameValues()
        {
            var address1 = new Address("123 Main St", "Springfield", "62701", "US");
            var address2 = new Address("123 Main St", "Springfield", "62701", "US");

            address1.Should().Be(address2);
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenDifferentStreet()
        {
            var address1 = new Address("123 Main St", "Springfield", "62701", "US");
            var address2 = new Address("456 Oak Ave", "Springfield", "62701", "US");

            address1.Should().NotBe(address2);
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenDifferentCity()
        {
            var address1 = new Address("123 Main St", "Springfield", "62701", "US");
            var address2 = new Address("123 Main St", "Chicago", "62701", "US");

            address1.Should().NotBe(address2);
        }

        [Fact]
        public void Equality_ShouldNotBeEqual_WhenDifferentCountry()
        {
            var address1 = new Address("123 Main St", "Springfield", "62701", "US");
            var address2 = new Address("123 Main St", "Springfield", "62701", "UK");

            address1.Should().NotBe(address2);
        }
    }
}
