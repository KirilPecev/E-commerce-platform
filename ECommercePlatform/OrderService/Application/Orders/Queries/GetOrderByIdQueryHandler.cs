
using MediatR;

using Microsoft.EntityFrameworkCore;

using OrderService.Application.Interfaces;

namespace OrderService.Application.Orders.Queries
{
    public class GetOrderByIdQueryHandler
        (IOrdersDbContext ordersDbContext) : IRequestHandler<GetOrderByIdQuery, OrderDto?>
    {
        public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
            => await ordersDbContext
                .Orders
                .AsNoTracking()
                .Where(o => o.Id == request.OrderId)
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
                .FirstOrDefaultAsync(cancellationToken);
    }
}
