namespace Contract.Models.Contract
{
    public class ContractRegulationModel
    {
        public ContractRegulationModel(Guid regulationId, Guid contractId)
        {
            RegulationId = regulationId;
            ContractId = contractId;
        }

        public ContractRegulationModel() {}

        public Guid RegulationId { get; init; }

        public Guid ContractId { get; init; }

        public DateTime AssignedAt { get; init; } = DateTime.Now;
    }
}