namespace CatalogService.Domain.Common
{
    public class Entity
    {
        public Guid Id { get; set; }

        public override bool Equals(object? obj)
            => obj is Entity other && Id == other.Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
