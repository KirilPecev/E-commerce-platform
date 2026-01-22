using CatalogService.Application.Products.Commands;
using CatalogService.Application.Products.Queries;
using CatalogService.Contracts.Requests;
using CatalogService.Contracts.Responses;

using ECommercePlatform.Identity;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController
        (IMediator mediator) : ControllerBase
    {
        [Authorize(Roles = Roles.Admin)]
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

        [Authorize(Roles = Roles.Admin)]
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

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            DeactivateProductCommand command = new DeactivateProductCommand(id);

            await mediator.Send(command);

            return NoContent();
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            ActivateProductCommand command = new ActivateProductCommand(id);

            await mediator.Send(command);

            return NoContent();
        }

        [Authorize(Roles = Roles.Admin)]
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

            Guid variantId = await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetProductVariantById),
                new { productId = id, variantId },
                new { ProductId = id, VariantId = variantId });
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

        [HttpGet("{productId:guid}/variants/{variantId:guid}")]
        public async Task<IActionResult> GetProductVariantById(Guid productId, Guid variantId)
        {
            ProductVariantDto? result = await mediator.Send(new GetProductVariantByIdQuery(productId, variantId));

            if (result is null) return NotFound();

            ProductVariantResponse response = new ProductVariantResponse(
                result.Id,
                result.Sku,
                result.Amount,
                result.Currency,
                result.Size,
                result.Color,
                result.StockQuantity);

            return Ok(response);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("{productId:guid}/variants/{variantId:guid}")]
        public async Task<IActionResult> DeleteProductVariant(Guid productId, Guid variantId)
        {
            DeleteProductVariantCommand command = new DeleteProductVariantCommand(productId, variantId);

            await mediator.Send(command);

            return NoContent();
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{productId:guid}/variants/{variantId:guid}")]
        public async Task<IActionResult> UpdateProductVariant(Guid productId, Guid variantId, [FromBody] UpdateProductVariantRequest request)
        {
            UpdateProductVariantCommand command = new UpdateProductVariantCommand(
                productId,
                variantId,
                request.Sku,
                request.Amount,
                request.Currency,
                request.StockQuantity,
                request.Size,
                request.Color);

            await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetProductVariantById),
                new { productId, variantId },
                new { ProductId = productId, VariantId = variantId });
        }
    }
}
