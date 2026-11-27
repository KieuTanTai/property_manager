namespace Identity.Application.Record.Cache
{
    public sealed record RecordIdentityCacheEntry<TFields>(Guid Id, TFields Fields);
}
