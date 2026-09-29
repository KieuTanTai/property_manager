namespace Statistic.Model.Contract
{
    public class ContractStatisticModel
    {
        private int total { get; set; }
        private int newContracts { get; set; }
        private int isContinues { get; set; }
        private int expireSoon { get; set; }
        public ContractStatisticModel()
        {
        }

        public ContractStatisticModel(int total, int newContracts, int isContinues, int expireSoon)
        {
            this.total = total;
            this.newContracts = newContracts;
            this.isContinues = isContinues;
            this.expireSoon = expireSoon;
        }
    }
}