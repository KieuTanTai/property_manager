namespace Identity.Infrastructure.Cache.CacheRecord
{
    public record RecordAccountAuthorizationCacheEntry(
        Guid AccountId,
        Guid[] RoleIds,
        Guid[] PermissionIds);
}