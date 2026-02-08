using MediatR;

namespace OrderService.Application.Orders.Commands
{
    public record FinalizeOrderCommand(
        Guid OrderId
        ) : IRequest;
}
