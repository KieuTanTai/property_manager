namespace Identity.Presentation.Record.Authorization;

public record IdListRequest(IReadOnlyList<Guid> Ids);

public record RoleChangeRequest(
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<Guid> PermissionIds);

public record PermissionChangeRequest(
    string Name,
    string? Description,
    bool IsActive);
