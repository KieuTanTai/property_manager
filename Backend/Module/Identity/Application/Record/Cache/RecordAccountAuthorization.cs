namespace Identity.Application.Record.Cache
{
    public record RecordAccountAuthorization(
        Guid[] RoleIds,
        Guid[] RolePermissionIds,
        Guid[] AdditionalPermissionIds
        );
}