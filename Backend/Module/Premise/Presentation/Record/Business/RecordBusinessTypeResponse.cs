namespace Premise.Presentation.Record.Business
{
    public record RecordBusinessTypeResponse(
        Guid BusinessTypeId,
        string BusinessTypeName,
        string? BusinessTypeDescription,
        bool BusinessTypeIsActive,
        DateTime BusinessTypeCreatedAt,
        DateTime BusinessTypeUpdatedAt
    );
}