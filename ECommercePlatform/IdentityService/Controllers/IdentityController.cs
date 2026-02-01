using System.Security.Claims;

using ECommercePlatform.Identity;

using IdentityService.Application.Identity.Commands;
using IdentityService.Contracts.Requests;
using IdentityService.Contracts.Responses;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentityController
         (IMediator mediator) : ControllerBase
    {
        [HttpPost(nameof(Register))]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            AuthResult result = await mediator.Send(
                new RegisterUserCommand(
                    request.Email,
                    request.Password));

            UserResponse response = new UserResponse(result.UserId, result.Token);

            return Ok(response);
        }

        [HttpPost(nameof(Login))]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            AuthResult result = await mediator.Send(
                new LoginUserCommand(
                    request.Email,
                    request.Password));

            UserResponse response = new UserResponse(result.UserId, result.Token);

            return Ok(result);
        }

        [Authorize]
        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {

            await mediator.Send(
                new ChangePasswordCommand(
                    Guid.Parse(User.FindFirstValue("id")!),
                    request.CurrentPassword,
                    request.NewPassword));

            return NoContent();
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{userId:guid}/roles")]
        public async Task<IActionResult> ChangeRoles(Guid userId, ChangeRoleRequest request)
        {
            await mediator.Send(
                new ChangeUserRoleCommand(userId, request.Roles));

            return NoContent();
        }
    }
}
