namespace CatalogService.Contracts.Responses
{
    public class ProductResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public decimal Price { get; init; }
    }
}
