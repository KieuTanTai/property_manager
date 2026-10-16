namespace Premise.Presentations.Record.Premise
{
    public record RecordPremiseBusinessTypeAssignmentResponse(
        Guid PremiseId,
        Guid BusinessTypeId,
        DateTime AssignedAt
    );
}
