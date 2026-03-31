using ECommercePlatform.Domain;

using FluentAssertions;

namespace ECommercePlatform.Tests
{
    public class ValueObjectTests
    {
        [Fact]
        public void Equals_WithSameComponents_ShouldReturnTrue()
        {
            var vo1 = new TestValueObject("A", 1);
            var vo2 = new TestValueObject("A", 1);

            vo1.Equals(vo2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentComponents_ShouldReturnFalse()
        {
            var vo1 = new TestValueObject("A", 1);
            var vo2 = new TestValueObject("B", 2);

            vo1.Equals(vo2).Should().BeFalse();
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            var vo = new TestValueObject("A", 1);

            vo.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void Equals_WithNonValueObject_ShouldReturnFalse()
        {
            var vo = new TestValueObject("A", 1);

            vo.Equals("not a value object").Should().BeFalse();
        }

        [Fact]
        public void Equals_WithSelf_ShouldReturnTrue()
        {
            var vo = new TestValueObject("A", 1);

            vo.Equals(vo).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithPartiallyMatchingComponents_ShouldReturnFalse()
        {
            var vo1 = new TestValueObject("A", 1);
            var vo2 = new TestValueObject("A", 2);

            vo1.Equals(vo2).Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_WithSameComponents_ShouldReturnSameHash()
        {
            var vo1 = new TestValueObject("A", 1);
            var vo2 = new TestValueObject("A", 1);

            vo1.GetHashCode().Should().Be(vo2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentComponents_ShouldReturnDifferentHash()
        {
            var vo1 = new TestValueObject("A", 1);
            var vo2 = new TestValueObject("B", 2);

            vo1.GetHashCode().Should().NotBe(vo2.GetHashCode());
        }

        [Fact]
        public void Equals_WithDifferentValueObjectSubtype_SameComponents_ShouldReturnTrue()
        {
            var vo1 = new TestValueObject("A", 1);
            var vo2 = new AnotherTestValueObject("A", 1);

            vo1.Equals(vo2).Should().BeTrue();
        }

        private class TestValueObject(string name, int value) : ValueObject
        {
            public string Name { get; } = name;

            public int Value { get; } = value;

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Name;
                yield return Value;
            }
        }

        private class AnotherTestValueObject(string name, int value) : ValueObject
        {
            public string Name { get; } = name;

            public int Value { get; } = value;

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Name;
                yield return Value;
            }
        }
    }
}
