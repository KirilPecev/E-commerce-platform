using MediatR;

namespace IdentityService.Application.Identity.Commands
{
    public record LoginUserCommand(
        string Email,
        string Password
        ) : IRequest<AuthResult>;
}
