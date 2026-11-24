using Premise.Presentation.Record.Location;

namespace Premise.Presentation.Record.Premise
{
    public record RecordPremiseDetailsResponse(
        RecordPremiseResponse Premise,
        RecordLocationResponse? Location,
        IReadOnlyList<RecordPremiseImageResponse> Images,
        IReadOnlyList<RecordPremiseBusinessTypeResponse> BusinessTypes
    );
}