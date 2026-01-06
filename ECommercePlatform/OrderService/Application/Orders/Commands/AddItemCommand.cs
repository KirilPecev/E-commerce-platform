using MediatR;

namespace OrderService.Application.Orders.Commands
{
    public record AddItemCommand(
        Guid OrderId,
        Guid ProductId,
        Guid ProductVariantId,
        string ProductName,
        decimal Price,
        string Currency,
        int Quantity
        ) : IRequest;
}