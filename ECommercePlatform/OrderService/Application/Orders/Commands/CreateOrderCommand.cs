using MediatR;

namespace OrderService.Application.Orders.Commands
{
    public record CreateOrderCommand(Guid CustomerId) : IRequest<Guid>;
}
