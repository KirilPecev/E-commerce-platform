using CatalogService.Application.Products.Commands;
using CatalogService.Application.Products.Queries;
using CatalogService.Contracts.Requests;
using CatalogService.Contracts.Responses;

using MassTransit.Configuration;

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
            ProductDto? result = await mediator.Send(new GetProductByIdQuery(id));

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<ProductDto> results = await mediator.Send(new GetAllProductsQuery());

            IEnumerable<ProductResponse> responses = results
                .Select(result => new ProductResponse(
                    result.Id,
                    result.Name,
                    result.Amount,
                    result.Currency
                ));

            return Ok(responses);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            UpdateProductCommand command = new UpdateProductCommand(
                id,
                request.Name,
                request.Amount,
                request.Currency,
                request.CategoryId,
                request.Description);

            await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new { Id = id });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            DeactivateProductCommand command = new DeactivateProductCommand(id);

            await mediator.Send(command);

            return NoContent();
        }

        [HttpPost("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            ActivateProductCommand command = new ActivateProductCommand(id);

            await mediator.Send(command);

            return NoContent();
        }

        [HttpPost("{id:guid}/variants")]
        public async Task<IActionResult> AddVariant(Guid id, [FromBody] AddProductVariantRequest request)
        {
            AddProductVariantCommand command = new AddProductVariantCommand(
                id,
                request.Sku,
                request.Amount,
                request.Currency,
                request.StockQuantity,
                request.Size,
                request.Color);

            await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new { Id = id });
        }

        [HttpGet("{id:guid}/variants")]
        public async Task<IActionResult> GetProductVariants(Guid id)
        {
            IEnumerable<ProductVariantDto> results = await mediator.Send(new GetProductVariantsQuery(id));

            IEnumerable<ProductVariantResponse> responses = results
                .Select(result => new ProductVariantResponse(
                    result.Id,
                    result.Sku,
                    result.Amount,
                    result.Currency,
                    result.Size,
                    result.Color,
                    result.StockQuantity
                ));

            return Ok(responses);
        }
    }
}
