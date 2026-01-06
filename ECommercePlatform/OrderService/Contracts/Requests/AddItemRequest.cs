namespace OrderService.Contracts.Requests
{
    public record AddItemRequest(
        Guid ProductId,
        Guid ProductVariantId,
        string ProductName,
        decimal Price,
        string Currency,
        int Quantity
        );
}