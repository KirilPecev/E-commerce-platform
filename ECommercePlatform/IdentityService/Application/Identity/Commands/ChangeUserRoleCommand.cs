using MediatR;

namespace IdentityService.Application.Identity.Commands
{
    public record ChangeUserRoleCommand(
        Guid UserId,
        string[] Roles
        ) : IRequest;
}
