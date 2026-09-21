namespace Premise.Models.Product
{
    public class ProductBusinessTypeModel
    {
        public ProductBusinessTypeModel() {}

        public ProductBusinessTypeModel(Guid productId, Guid businessTypeId)
        {
            ProductId = productId;
            BusinessTypeId = businessTypeId;
        }

        public Guid ProductId { get; init; }

        public Guid BusinessTypeId { get; init; }

        public DateTime AssignedAt { get; init; } = DateTime.Now;
    }
}