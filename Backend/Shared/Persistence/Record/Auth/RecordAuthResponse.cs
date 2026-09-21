namespace Shared.Persistence.Record.Auth
{
    // fields of profile null in case profile not create when register (manual create profile)   
    public record RecordAuthResponse(
        Guid AccountId,
        string Email,
        bool IsActive,
        List<string> RoleCodes,
        List<string> PermissionCodes,
        DateTime AccountCreatedAt,
        DateTime AccountUpdatedAt
    );
}