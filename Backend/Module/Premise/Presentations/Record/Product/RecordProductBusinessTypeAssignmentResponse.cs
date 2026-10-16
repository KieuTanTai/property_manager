namespace Premise.Presentations.Record.Product
{
    public record RecordProductBusinessTypeAssignmentResponse(
        Guid ProductId,
        Guid BusinessTypeId,
        DateTime AssignedAt
    );
}
