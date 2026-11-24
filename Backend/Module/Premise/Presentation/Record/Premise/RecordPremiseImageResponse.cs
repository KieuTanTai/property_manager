namespace Premise.Presentation.Record.Premise
{
    public record RecordPremiseImageResponse(
        int ImageId,
        Guid PremiseId,
        string ImageUrl,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}