namespace IdentityService.Contracts.Requests
{
    public record ChangePasswordRequest(
        Guid UserId,
        string CurrentPassword,
        string NewPassword);
}
