namespace CatalogService.Contracts.Requests
{
    public record AddProductVariantRequest()
    {
        public string Sku { get; internal set; }
        public decimal Amount { get; internal set; }
        public string Currency { get; internal set; }
        public int StockQuantity { get; internal set; }
        public string? Size { get; internal set; }
        public string? Color { get; internal set; }
    }
}