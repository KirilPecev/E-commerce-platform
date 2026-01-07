using InventoryService.Application.Inventory.Queries;
using InventoryService.Contracts.Responses;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController
        (IMediator mediator) : ControllerBase
    {
        [HttpGet("{productId:guid}")]
        public async Task<IActionResult> GetAvaiableStocksForProduct(Guid productId)
        {
            GetProductStocksQuery query = new GetProductStocksQuery(productId);

            List<ProductStockDto> stocks = await mediator.Send(query);

            List<ProductStocksResponse> result = stocks
                .Select(stock => new ProductStocksResponse(
                    stock.ProductId,
                    stock.ProductVariantId,
                    stock.QuantityAvailable,
                    stock.QuantityReserved
                ))
                .ToList();

            return Ok(stocks);
        }
    }
}
