namespace Premise.Presentation.Record.Product
{
    public record RecordProductBusinessTypeAssignmentResponse(
        Guid ProductId,
        Guid BusinessTypeId,
        DateTime AssignedAt
    );
}