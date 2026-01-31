using MediatR;

namespace IdentityService.Application.Identity.Commands
{
    public record ChangePasswordCommand(
        Guid UserId,
        string CurrentPassword,
        string NewPassword
        ) : IRequest;
}
