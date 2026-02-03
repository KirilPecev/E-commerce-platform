
using MediatR;

using Microsoft.EntityFrameworkCore;

using OrderService.Infrastructure.Persistence;

namespace OrderService.Application.Orders.Queries
{
    public class GetOrdersByCustomerQueryHandler
        (OrdersDbContext ordersDbContext) : IRequestHandler<GetOrdersByCustomerQuery, List<OrderDto>>
    {
        public async Task<List<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
            => await ordersDbContext
                .Orders
                .AsNoTracking()
                .Where(order => order.CustomerId == request.CustomerId)
                .Select(o => new OrderDto(
                    o.Id,
                    o.CustomerId,
                    o.CreatedAt,
                    o.Status,
                    o.TotalPrice,
                    o.ShippingAddress,
                    o.CancellationReason,
                    o.ShippedAt,
                    o.TrackingNumber,
                    new List<OrderItemDto>(
                        o.Items.Select(i => new OrderItemDto(
                            i.Id,
                            i.ProductVariantId,
                            i.ProductName,
                            i.UnitPrice.Amount,
                            i.UnitPrice.Currency,
                            i.Quantity,
                            i.TotalPrice
                        )).ToList())
                ))
                .ToListAsync(cancellationToken);
    }
}
