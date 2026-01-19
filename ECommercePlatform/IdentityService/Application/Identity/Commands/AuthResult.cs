namespace IdentityService.Application.Identity.Commands
{
    public record AuthResult(
        Guid UserId,
        string token
        );
}
