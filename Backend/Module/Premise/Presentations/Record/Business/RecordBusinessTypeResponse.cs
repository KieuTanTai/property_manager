namespace Premise.Presentations.Record.Business;

public record RecordBusinessTypeResponse(
    Guid BusinessTypeId,
    string BusinessTypeName,
    string? BusinessTypeDescription,
    bool BusinessTypeIsActive,
    DateTime BusinessTypeCreatedAt,
    DateTime BusinessTypeUpdatedAt
);
