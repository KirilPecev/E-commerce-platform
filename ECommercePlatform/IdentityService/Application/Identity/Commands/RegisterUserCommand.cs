using MediatR;

namespace IdentityService.Application.Identity.Commands
{
    public record RegisterUserCommand(
        string Email,
        string Password
        ) : IRequest<AuthResult>;
}
