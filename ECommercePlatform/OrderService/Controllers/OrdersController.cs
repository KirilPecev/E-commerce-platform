using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController
        (IMediator mediator): ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            // Implementation for creating an order
            return Ok();
        }
    }
}
