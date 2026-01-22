using ECommercePlatform.Identity;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PaymentService.Application.Payments.Command;
using PaymentService.Contracts.Requests;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController
        (IMediator mediator) : ControllerBase
    {
        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpPost("pay")]
        public async Task<IActionResult> PayWithCard([FromBody] PayWithCardRequest request, CancellationToken cancellationToken)
        {
            PayWithCardCommand command = new PayWithCardCommand(
                request.PaymentId,
                request.CardNumber,
                request.CardHolder,
                request.Expiry,
                request.Cvv);

            await mediator.Send(command, cancellationToken);

            return Accepted();
        }
    }
}
