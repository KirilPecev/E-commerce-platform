using ECommercePlatform.Identity;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OrderService.Application.Orders.Commands;
using OrderService.Application.Orders.Queries;
using OrderService.Contracts.Requests;
using OrderService.Contracts.Responses;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController
        (IMediator mediator) : ControllerBase
    {
        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            CreateOrderCommand command = new CreateOrderCommand(
                request.CustomerId,
                request.ProductId,
                request.ProductVariantId,
                request.ProductName,
                request.Price,
                request.Currency,
                request.Quantity);

            Guid orderId = await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = orderId },
                new { Id = orderId });
        }

        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            GetOrderByIdQuery query = new GetOrderByIdQuery(id);

            OrderDto? order = await mediator.Send(query);

            if (order == null)
            {
                return NotFound();
            }

            OrderResponse orderResponse = new OrderResponse(
                order.Id,
                order.CustomerId,
                order.CreatedAt,
                order.Status,
                order.TotalPrice,
                order.ShippingAddress,
                order.CancellationReason,
                order.ShippedAt,
                order.TrackingNumber,
                order.Items.Select(i => new OrderItemResponse(i.Id, i.ProductVariantId, i.ProductName, i.UnitPrice, i.Currency, i.Quantity, i.TotalPrice)).ToList()
            );

            return Ok(order);
        }

        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpPost("{orderId:guid}/items")]

        public async Task<IActionResult> AddItemToOrder(Guid orderId, AddItemRequest request)
        {
            AddItemCommand command = new AddItemCommand(
                orderId,
                request.ProductId,
                request.ProductVariantId,
                request.ProductName,
                request.Price,
                request.Currency,
                request.Quantity);

            await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = orderId },
                new { Id = orderId });
        }

        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpPut("{orderId:guid}/address")]
        public async Task<IActionResult> SetAddress(Guid orderId, SetOrderAddressRequest request)
        {
            SetOrderAddressCommand command = new SetOrderAddressCommand(
                orderId,
                request.Street,
                request.City,
                request.ZipCode,
                request.Country);

            await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = orderId },
                new { Id = orderId });
        }

        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
        public async Task<IActionResult> RemoveItemFromOrder(Guid orderId, Guid itemId)
        {
            RemoveItemCommand command = new RemoveItemCommand(orderId, itemId);

            await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = orderId },
                new { Id = orderId });
        }

        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpPost("{orderId:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid orderId, CancelOrderRequest request)
        {
            CancelOrderCommand command = new CancelOrderCommand(
                orderId,
                request.Reason);

            await mediator.Send(command);

            return NoContent();
        }

        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpPost("{orderId:guid}/pay")]
        public async Task<IActionResult> Pay(Guid orderId)
        {
            PayOrderCommand command = new PayOrderCommand(orderId);

            await mediator.Send(command);

            return NoContent();
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("{orderId:guid}/ship")]
        public async Task<IActionResult> Ship(Guid orderId, ShipOrderRequest request)
        {
            ShipOrderCommand command = new ShipOrderCommand(orderId, request.TrackingNumber);

            await mediator.Send(command);

            return NoContent();
        }

        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetAllOrdersForCustomer(Guid customerId)
        {
            GetOrdersByCustomerQuery query = new GetOrdersByCustomerQuery(customerId);

            List<OrderDto> orders = await mediator.Send(query);

            List<OrderResponse> orderResponses = orders.Select(order => new OrderResponse(
                order.Id,
                order.CustomerId,
                order.CreatedAt,
                order.Status,
                order.TotalPrice,
                order.ShippingAddress,
                order.CancellationReason,
                order.ShippedAt,
                order.TrackingNumber,
                order.Items.Select(i => new OrderItemResponse(i.Id, i.ProductVariantId, i.ProductName, i.UnitPrice, i.Currency, i.Quantity, i.TotalPrice)).ToList()
            ))
                .ToList();

            return Ok(orderResponses);
        }
    }
}
