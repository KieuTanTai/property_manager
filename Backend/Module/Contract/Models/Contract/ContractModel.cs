using Contract.Models.Invoice;
using Contract.Utils.Enum;

namespace Contract.Models.Contract
{
    public class ContractModel
    {
        public ContractModel(Guid accountId, decimal deposit, decimal rentalPrice, DateTime premiseReturnDate)
        {
            ContractAccountId = accountId;
            ContractDeposit = deposit;
            ContractRentalPrice = rentalPrice;
            ContractPremiseReturnDate = premiseReturnDate;
        }

        public ContractModel(
            Guid contractId,
            Guid accountId,
            decimal deposit,
            decimal rentalPrice,
            DateTime premiseReturnDate,
            EContractStatus status,
            DateTime? terminationDate)
        {
            ContractId = contractId;
            ContractAccountId = accountId;
            ContractDeposit = deposit;
            ContractRentalPrice = rentalPrice;
            ContractPremiseReturnDate = premiseReturnDate;
            ContractStatus = status;
            ContractTerminationDate = terminationDate;
        }

        public ContractModel() {}

        public Guid ContractId { get; init; }

        public Guid ContractAccountId { get; private set; }

        public decimal ContractDeposit { get; private set; }

        public decimal ContractRentalPrice { get; private set; }

        public DateTime ContractPremiseReturnDate { get; private set; }

        public EContractStatus ContractStatus { get; private set; } = EContractStatus.PendingSignature;

        public DateTime? ContractTerminationDate { get; private set; }

        public DateTime ContractCreatedAt { get; init; } = DateTime.Now;

        public DateTime ContractUpdatedAt { get; private set; } = DateTime.Now;
        
        public IReadOnlyList<ContractViolationModel> ContractViolations { get; private set; } = new List<ContractViolationModel>();
        public IReadOnlyList<MonthlyInvoiceModel> ContractInvoices { get; private set; } = new List<MonthlyInvoiceModel>();
        public IReadOnlyList<RegulationModel> ContractRegulations { get; private set; } = new List<RegulationModel>();
        
        #region Setter

        public void SetContractAccountId(Guid accountId)
        {
            ContractAccountId = accountId;
        }

        public void SetContractDeposit(decimal deposit)
        {
            ContractDeposit = deposit;
        }

        public void SetContractRentalPrice(decimal rentalPrice)
        {
            ContractRentalPrice = rentalPrice;
        }

        public void SetContractPremiseReturnDate(DateTime premiseReturnDate)
        {
            ContractPremiseReturnDate = premiseReturnDate;
        }

        public void SetContractStatus(EContractStatus status)
        {
            ContractStatus = status;
        }

        public void SetContractTerminationDate(DateTime? terminationDate)
        {
            ContractTerminationDate = terminationDate;
        }
        
        public void SetContractViolations(IReadOnlyList<ContractViolationModel> violations)
        {
            ContractViolations = violations;
        }

        public void SetContractInvoices(IReadOnlyList<MonthlyInvoiceModel> invoices)
        {
            ContractInvoices = invoices;
        }
        
        public void SetContractRegulations(IReadOnlyList<RegulationModel> regulations)
        {
            ContractRegulations = regulations;
        }
        #endregion
    }
}