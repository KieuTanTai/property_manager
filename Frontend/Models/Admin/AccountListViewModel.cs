namespace Frontend.Models.Admin;

public sealed record AdminAccountRow(
    string AccountId,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt);

public sealed record AccountListViewModel(IReadOnlyList<AdminAccountRow> Accounts);
