using IdentityService.Application.Identity.Commands;
using IdentityService.Contracts.Requests;
using IdentityService.Contracts.Responses;

using MediatR;

using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentityController
         (IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            AuthResult result = await mediator.Send(
                new RegisterUserCommand(
                    request.Email,
                    request.Password));

            UserResponse response = new UserResponse(result.UserId, result.Token);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            AuthResult result = await mediator.Send(
                new LoginUserCommand(
                    request.Email,
                    request.Password));

            UserResponse response = new UserResponse(result.UserId, result.Token);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            await mediator.Send(
                new ChangePasswordCommand(
                    request.UserId,
                    request.CurrentPassword,
                    request.NewPassword));

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(ChangeRoleRequest request)
        {
            await mediator.Send(
                new ChangeUserRoleCommand(
                    request.UserId,
                    request.NewRole));

            return NoContent();
        }
    }
}
