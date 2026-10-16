namespace Premise.Presentations.Record.Product
{
    public record RecordProductBusinessTypeResponse(
        Guid BusinessTypeId,
        string Name,
        string? Description,
        bool IsActive
    );
}
