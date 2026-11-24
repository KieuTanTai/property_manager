namespace Premise.Presentation.Record.Premise
{
    public record RecordPremiseBusinessTypeAssignmentResponse(
        Guid PremiseId,
        Guid BusinessTypeId,
        DateTime AssignedAt
    );
}