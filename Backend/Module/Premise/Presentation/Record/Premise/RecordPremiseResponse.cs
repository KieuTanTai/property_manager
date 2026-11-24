using Shared.Enum;

namespace Premise.Presentation.Record.Premise
{
    public record RecordPremiseResponse(
        Guid PremiseId,
        string PremiseName,
        Guid PremiseLocationId,
        EPremiseStatus PremiseStatus,
        int PremisePosition,
        int PremiseFloor,
        string PremiseArea,
        string? PremiseDescription,
        DateTime PremiseCreatedAt,
        DateTime PremiseUpdatedAt
    );
}