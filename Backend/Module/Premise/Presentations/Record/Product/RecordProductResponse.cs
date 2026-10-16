namespace Premise.Presentations.Record.Product
{
    public record RecordProductResponse(
        Guid ProductId,
        string ProductName,
        string? ProductDescription,
        DateTime ProductCreatedAt,
        DateTime ProductUpdatedAt
    );
}
