namespace Premise.Presentations.Record.Product
{
    public record RecordProductWithBusinessTypeIdResponse(
        Guid ProductId,
        Guid BusinessTypeId,
        string ProductName,
        string? ProductDescription,
        DateTime ProductCreatedAt,
        DateTime ProductUpdatedAt
    );
}
