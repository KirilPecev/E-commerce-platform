using CatalogService.Application.Products.Commands;
using CatalogService.Application.Products.Queries;
using CatalogService.Contracts.Requests;
using CatalogService.Contracts.Responses;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController
        (IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            CreateProductCommand command = new CreateProductCommand(
                request.Name,
                request.Amount,
                request.Currency,
                request.CategoryId,
                request.Description);

            Guid productId = await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = productId },
                new { Id = productId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            ProductDto result = await mediator.Send(new GetProductByIdQuery(id));

            if (result is null)
                return NotFound();

            ProductResponse response = new ProductResponse(
                result.Id,
                result.Name,
                result.Amount,
                result.Currency
            );

            return Ok(response);
        }
    }
}
