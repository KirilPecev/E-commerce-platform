using MediatR;

namespace OrderService.Application.Orders.Commands
{
    public record CreateOrderCommand(
        Guid CustomerId,
        Guid ProductId,
        Guid ProductVariantId,
        string ProductName,
        decimal Price,
        string Currency,
        int Quantity
        ) : IRequest<Guid>;
}
