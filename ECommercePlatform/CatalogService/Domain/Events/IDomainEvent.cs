namespace CatalogService.Domain.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}
