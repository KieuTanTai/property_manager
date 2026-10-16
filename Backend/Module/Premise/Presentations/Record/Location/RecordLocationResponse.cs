namespace Premise.Presentations.Record.Location
{
    public record RecordLocationResponse(
        Guid LocationId,
        string LocationAddress,
        bool LocationIsActive,
        DateTime LocationCreatedAt,
        DateTime LocationUpdatedAt
    );
}
