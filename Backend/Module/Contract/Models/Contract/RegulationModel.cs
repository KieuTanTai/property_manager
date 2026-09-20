using System.ComponentModel.DataAnnotations;

namespace Contract.Models.Contract
{
    public class RegulationModel
    {
        public RegulationModel(string regulationName)
        {
            RegulationName = regulationName;
        }

        public RegulationModel(
            Guid regulationId,
            string regulationName,
            string? regulationDescription,
            decimal? regulationFineAmount,
            bool regulationIsActive)
        {
            RegulationId = regulationId;
            RegulationName = regulationName;
            RegulationDescription = regulationDescription;
            RegulationFineAmount = regulationFineAmount;
            RegulationIsActive = regulationIsActive;
        }

        public RegulationModel() {}

        public Guid RegulationId { get; init; }

        [MaxLength(50)] public string RegulationName { get; private set; } = string.Empty;

        [MaxLength(255)] public string? RegulationDescription { get; private set; }

        public decimal? RegulationFineAmount { get; private set; }

        public bool RegulationIsActive { get; private set; } = true;

        public DateTime RegulationCreatedAt { get; init; } = DateTime.Now;

        public DateTime RegulationUpdatedAt { get; private set; } = DateTime.Now;

        #region Setter

        public void SetRegulationName(string name)
        {
            RegulationName = name;
        }

        public void SetRegulationDescription(string? description)
        {
            RegulationDescription = description;
        }

        public void SetRegulationFineAmount(decimal? fineAmount)
        {
            RegulationFineAmount = fineAmount;
        }

        public void SetRegulationIsActive(bool isActive)
        {
            RegulationIsActive = isActive;
        }

        #endregion
    }
}