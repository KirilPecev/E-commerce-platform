namespace IdentityService.Contracts.Requests
{
    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword);
}
