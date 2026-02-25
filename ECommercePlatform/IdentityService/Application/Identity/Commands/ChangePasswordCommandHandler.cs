
using IdentityService.Application.Exceptions;
using IdentityService.Infrastructure;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Identity.Commands
{
    public class ChangePasswordCommandHandler
        (UserManager<User> userManager) : IRequestHandler<ChangePasswordCommand>
    {
        public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            User? user = await userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
            {
                throw new IdentityException("User not found.");
            }

            IdentityResult result = await userManager.ChangePasswordAsync(
                user: user,
                currentPassword: request.CurrentPassword,
                newPassword: request.NewPassword);

            if (!result.Succeeded)
            {
                throw new IdentityException("Password change failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
