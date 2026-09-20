using System.ComponentModel.DataAnnotations;

namespace Contract.Models.Contract
{
    public class ContractViolationModel
    {
        public ContractViolationModel(Guid contractId, string violationContent)
        {
            ContractId = contractId;
            ViolationContent = violationContent;
        }

        public ContractViolationModel(
            Guid violationId,
            Guid contractId,
            string violationContent,
            decimal? violationPenaltyAmount,
            DateTime violationDate,
            DateTime violationDueDate,
            bool violationIsResolved)
        {
            ViolationId = violationId;
            ContractId = contractId;
            ViolationContent = violationContent;
            ViolationPenaltyAmount = violationPenaltyAmount;
            ViolationDate = violationDate;
            ViolationDueDate = violationDueDate;
            ViolationIsResolved = violationIsResolved;
        }

        public ContractViolationModel() {}

        public Guid ViolationId { get; init; }

        public Guid ContractId { get; private set; }

        [MaxLength(150)] public string ViolationContent { get; private set; } = string.Empty;

        public decimal? ViolationPenaltyAmount { get; private set; }

        public DateTime ViolationDate { get; private set; } = DateTime.Now;

        public DateTime ViolationDueDate { get; private set; } = DateTime.Now.AddDays(7);

        public bool ViolationIsResolved { get; private set; }

        #region Setter
        
        public void SetViolationContent(string violationContent)
        {
            ViolationContent = violationContent;
        }
        
        public void SetViolationPenaltyAmount(decimal? violationPenaltyAmount)
        {
            ViolationPenaltyAmount = violationPenaltyAmount;
        }
        
        public void SetViolationDate(DateTime violationDate)
        {
            ViolationDate = violationDate;
        }
        
        public void SetViolationDueDate(DateTime violationDueDate)
        {
            ViolationDueDate = violationDueDate;
        }
        
        public void SetViolationIsResolved(bool violationIsResolved)
        {
            ViolationIsResolved = violationIsResolved;
        }

        #endregion
    }
}