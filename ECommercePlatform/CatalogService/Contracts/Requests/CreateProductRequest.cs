namespace CatalogService.Contracts.Requests
{
    public class CreateProductRequest
    {
        public string Name { get; init; } = default!;
        public decimal Price { get; init; }
    }
}
