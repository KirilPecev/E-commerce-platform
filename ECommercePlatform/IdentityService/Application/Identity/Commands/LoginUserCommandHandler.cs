
using IdentityService.Infrastructure;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Identity.Commands
{
    public class LoginUserCommandHandler
        (UserManager<User> userManager,
        IJwtTokenGenerator jwt) : IRequestHandler<LoginUserCommand, AuthResult>
    {
        public async Task<AuthResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            User? user = await userManager.FindByEmailAsync(request.Email)
                ?? throw new UnauthorizedAccessException("Invalid login attempt.");

            bool isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException();
            }

            IList<string> roles = await userManager.GetRolesAsync(user);

            string token = jwt.GenerateToken(user, roles);

            return new AuthResult(
                user.Id,
                token
            );
        }
    }
}
