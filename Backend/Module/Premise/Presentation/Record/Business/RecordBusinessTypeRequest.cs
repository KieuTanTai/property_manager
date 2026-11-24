namespace Premise.Presentation.Record.Business
{
    public record RecordBusinessTypeRequest(
        string BusinessTypeName,
        string? BusinessTypeDescription,
        bool BusinessTypeIsActive = true);
}