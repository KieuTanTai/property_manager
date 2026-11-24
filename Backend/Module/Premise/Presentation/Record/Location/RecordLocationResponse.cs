namespace Premise.Presentation.Record.Location
{
    public record RecordLocationResponse(
        Guid LocationId,
        string LocationAddress,
        DateTime LocationCreatedAt,
        DateTime LocationUpdatedAt
    );
}