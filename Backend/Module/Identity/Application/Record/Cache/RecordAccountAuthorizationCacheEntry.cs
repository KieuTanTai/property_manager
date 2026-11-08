namespace Identity.Application.Record.Cache
{
    public record RecordAccountAuthorizationCacheEntry(
        Guid AccountId,
        Guid[] RoleIds,
        Guid[] PermissionIds);
}