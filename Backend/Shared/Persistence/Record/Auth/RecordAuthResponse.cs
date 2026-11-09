namespace Shared.Persistence.Record.Auth
{
    public record RecordAuthResponse(
        Guid AccountId,
        string Email,
        bool IsActive,
        List<string> RoleCodes,
        List<string> RolePermissionCodes,
        List<string> AdditionalPermissionCodes,
        DateTime AccountCreatedAt,
        DateTime AccountUpdatedAt
    );
}
