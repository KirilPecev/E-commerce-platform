using CatalogService.Application.Categories.Commands;
using CatalogService.Application.Categories.Queries;
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
    public class CategoriesController
        (IMediator mediator) : ControllerBase
    {
        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            CreateCategoryCommand command = new CreateCategoryCommand(
                request.Name,
                request.Description);

            Guid categoryId = await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = categoryId },
                new { Id = categoryId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            CategoryDto? result = await mediator.Send(new GetCaterogyByIdQuery(id));

            if (result is null)
                return NotFound();

            CategoryResponse response = new CategoryResponse(
                result.Id,
                result.Name,
                result.Description
            );

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<CategoryDto> results = await mediator.Send(new GetAllCategoriesQuery());

            IEnumerable<CategoryResponse> responses = results
                .Select(result => new CategoryResponse(
                    result.Id,
                    result.Name,
                    result.Description
                ));

            return Ok(responses);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            UpdateCategoryCommand command = new UpdateCategoryCommand(
                id,
                request.Name,
                request.Description);

            await mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new { Id = id });
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            DeleteCategoryCommand command = new DeleteCategoryCommand(id);

            await mediator.Send(command);

            return NoContent();
        }
    }
}
