namespace Premise.Presentation.Record.Premise
{
    public record RecordPremiseBusinessTypeResponse(
        Guid BusinessTypeId,
        string Name,
        string? Description,
        bool IsActive
    );
}