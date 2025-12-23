using CatalogService.Application.Products.Commands;
using CatalogService.Contracts.Requests;

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
                request.Price);

            Guid productId = await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = productId },
                new { Id = productId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Query implementation later
            return Ok();
        }
    }
}
