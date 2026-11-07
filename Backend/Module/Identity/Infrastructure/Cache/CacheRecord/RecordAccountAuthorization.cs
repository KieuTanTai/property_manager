namespace Identity.Infrastructure.Cache.CacheRecord
{
    public record RecordAccountAuthorization(
        Guid[] RoleIds,
        Guid[] RolePermissionIds,
        Guid[] AdditionalPermissionIds
        );
}