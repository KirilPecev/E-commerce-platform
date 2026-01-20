namespace IdentityService.Contracts.Responses
{
    public record UserResponse(
        Guid UserId,
        string Token);
}
