namespace IdentityService.Contracts.Requests
{
    public record ChangeRoleRequest(
        Guid UserId,
        string NewRole
        );
}
