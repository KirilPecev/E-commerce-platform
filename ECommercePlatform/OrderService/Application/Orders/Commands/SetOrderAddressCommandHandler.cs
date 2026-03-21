using MediatR;

using Microsoft.EntityFrameworkCore;

using OrderService.Application.Interfaces;
using OrderService.Domain.Aggregates;
using OrderService.Domain.ValueObjects;

namespace OrderService.Application.Orders.Commands
{
    public class SetOrderAddressCommandHandler
        (IOrdersDbContext ordersDbContext) : IRequestHandler<SetOrderAddressCommand>
    {
        public async Task Handle(SetOrderAddressCommand request, CancellationToken cancellationToken)
        {
            Order? order = await ordersDbContext
                 .Orders
                 .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
            }

            Address address = new Address(
                request.Street,
                request.City,
                request.ZipCode,
                request.Country);

            order.SetShippingAddress(address);

            await ordersDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
