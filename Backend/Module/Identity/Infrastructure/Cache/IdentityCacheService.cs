using Identity.Infrastructure.Cache.CacheRecord;
using Identity.Models.Permission;
using Identity.Models.Role;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.Caching;

namespace Identity.Infrastructure.Cache
{
    public sealed class IdentityCacheService(HybridCache cache) 
    {
        private const string RolesKey = "identity:roles:v1";
        private const string PermissionsKey = "identity:permissions:v1";
        private const string AccountAuthorizationKeyPrefix = "identity:account-authorization:v1:";

        private static readonly HybridCacheEntryOptions ReferenceDataOptions = new()
        {
            Expiration = TimeSpan.FromHours(12),
            LocalCacheExpiration = TimeSpan.FromMinutes(30)
        };

        private static readonly HybridCacheEntryOptions AccountOptions = new()
        {
            Expiration = TimeSpan.FromMinutes(15),
            LocalCacheExpiration = TimeSpan.FromMinutes(5)
        };

        public ValueTask<RoleModel[]> GetRolesAsync(
            Func<CancellationToken, Task<IReadOnlyList<RoleModel>>> loader,
            CancellationToken cancellationToken = default) =>
            cache.GetOrCreateAsync(
                RolesKey,
                async token => (await loader(token)).ToArray(),
                ReferenceDataOptions,
                cancellationToken: cancellationToken);

        public ValueTask<PermissionModel[]> GetPermissionsAsync(
            Func<CancellationToken, Task<IReadOnlyList<PermissionModel>>> loader,
            CancellationToken cancellationToken = default) =>
            cache.GetOrCreateAsync(
                PermissionsKey,
                async token => (await loader(token)).ToArray(),
                ReferenceDataOptions,
                cancellationToken: cancellationToken);

        public ValueTask<RecordAccountAuthorizationCacheEntry> GetAccountAuthorizationAsync(
            Guid accountId,
            Func<CancellationToken, Task<RecordAccountAuthorization>> loader,
            CancellationToken cancellationToken = default)
        {
            if (accountId == Guid.Empty)
            {
                throw new ArgumentException("Account ID is required.", nameof(accountId));
            }
            return cache.GetOrCreateAsync(
                AccountAuthorizationKey(accountId),
                async token =>
                {
                    var source = await loader(token);
                    return new RecordAccountAuthorizationCacheEntry(
                        accountId,
                        [.. source.RoleIds.Distinct()],
                        [.. source.RolePermissionIds.Concat(source.AdditionalPermissionIds).Distinct()]);
                },
                AccountOptions,
                cancellationToken: cancellationToken);
        }

        public ValueTask InvalidateRolesAsync(CancellationToken cancellationToken = default) =>
            cache.RemoveAsync(RolesKey, cancellationToken);

        public ValueTask InvalidatePermissionsAsync(CancellationToken cancellationToken = default) =>
            cache.RemoveAsync(PermissionsKey, cancellationToken);

        public ValueTask InvalidateAccountAuthorizationAsync(
            Guid accountId,
            CancellationToken cancellationToken = default) =>
            cache.RemoveAsync(AccountAuthorizationKey(accountId), cancellationToken);

        private static string AccountAuthorizationKey(Guid accountId) =>
            $"{AccountAuthorizationKeyPrefix}{accountId:N}";
    }
}
