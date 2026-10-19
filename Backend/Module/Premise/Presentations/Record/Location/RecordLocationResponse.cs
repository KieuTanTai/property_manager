namespace Premise.Presentations.Record.Location
{
    public record RecordLocationResponse(
        Guid LocationId,
        string LocationAddress,
        DateTime LocationCreatedAt,
        DateTime LocationUpdatedAt
    );
}
