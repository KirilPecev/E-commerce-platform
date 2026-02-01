
using IdentityService.Infrastructure;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Identity.Commands
{
    public class ChangeUserRoleCommandHandler
        (UserManager<User> userManager) : IRequestHandler<ChangeUserRoleCommand>
    {
        public async Task Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
        {
            User? user = await userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            await userManager.RemoveFromRolesAsync(user, await userManager.GetRolesAsync(user));

            await userManager.AddToRolesAsync(user, request.Roles);
        }
    }
}
