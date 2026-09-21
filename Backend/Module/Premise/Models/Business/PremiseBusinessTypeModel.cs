namespace Premise.Models.Business
{
    public class PremiseBusinessTypeModel
    {
        public PremiseBusinessTypeModel() {}

        public PremiseBusinessTypeModel(Guid premiseId, Guid businessTypeId)
        {
            PremiseId = premiseId;
            BusinessTypeId = businessTypeId;
        }

        public Guid PremiseId { get; init; }

        public Guid BusinessTypeId { get; init; }

        public DateTime AssignedAt { get; init; } = DateTime.UtcNow;
    }
}