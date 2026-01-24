
using ECommercePlatform.Identity;

using IdentityService.Application.Exceptions;
using IdentityService.Infrastructure;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Identity.Commands
{
    public class RegisterUserCommandHandler
        (UserManager<User> userManager,
        IJwtTokenGenerator jwt) : IRequestHandler<RegisterUserCommand, AuthResult>
    {
        public async Task<AuthResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            User user = new()
            {
                Email = request.Email,
                UserName = request.Email,
            };

            IdentityResult result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new IdentityException("User registration failed", result.Errors);
            }

            await userManager.AddToRoleAsync(user, Roles.Customer);

            string token = jwt.GenerateToken(user, new List<string>() { Roles.Customer });

            return new AuthResult(
                user.Id,
                token
            );
        }
    }
}