using ECommercePlatform.Domain;

using FluentAssertions;

namespace ECommercePlatform.Tests
{
    public class EntityTests
    {
        [Fact]
        public void Equals_WithSameId_ShouldReturnTrue()
        {
            var id = Guid.NewGuid();
            var entity1 = new TestEntity { Id = id };
            var entity2 = new TestEntity { Id = id };

            entity1.Equals(entity2).Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentId_ShouldReturnFalse()
        {
            var entity1 = new TestEntity { Id = Guid.NewGuid() };
            var entity2 = new TestEntity { Id = Guid.NewGuid() };

            entity1.Equals(entity2).Should().BeFalse();
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            var entity = new TestEntity { Id = Guid.NewGuid() };

            entity.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void Equals_WithNonEntityObject_ShouldReturnFalse()
        {
            var entity = new TestEntity { Id = Guid.NewGuid() };

            entity.Equals("not an entity").Should().BeFalse();
        }

        [Fact]
        public void Equals_WithSelf_ShouldReturnTrue()
        {
            var entity = new TestEntity { Id = Guid.NewGuid() };

            entity.Equals(entity).Should().BeTrue();
        }

        [Fact]
        public void GetHashCode_WithSameId_ShouldReturnSameHash()
        {
            var id = Guid.NewGuid();
            var entity1 = new TestEntity { Id = id };
            var entity2 = new TestEntity { Id = id };

            entity1.GetHashCode().Should().Be(entity2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_WithDifferentId_ShouldReturnDifferentHash()
        {
            var entity1 = new TestEntity { Id = Guid.NewGuid() };
            var entity2 = new TestEntity { Id = Guid.NewGuid() };

            entity1.GetHashCode().Should().NotBe(entity2.GetHashCode());
        }

        [Fact]
        public void Equals_WithDifferentEntitySubtype_SameShouldReturnTrue()
        {
            var id = Guid.NewGuid();
            var entity1 = new TestEntity { Id = id };
            var entity2 = new AnotherTestEntity { Id = id };

            entity1.Equals(entity2).Should().BeTrue();
        }

        private class TestEntity : Entity { }

        private class AnotherTestEntity : Entity { }
    }
}
