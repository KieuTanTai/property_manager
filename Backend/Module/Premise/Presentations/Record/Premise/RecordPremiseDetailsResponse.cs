using Premise.Presentations.Record.Location;

namespace Premise.Presentations.Record.Premise
{
    public record RecordPremiseDetailsResponse(
        RecordPremiseResponse Premise,
        RecordLocationResponse? Location,
        IReadOnlyList<RecordPremiseImageResponse> Images,
        IReadOnlyList<RecordPremiseBusinessTypeResponse> BusinessTypes
    );
}
