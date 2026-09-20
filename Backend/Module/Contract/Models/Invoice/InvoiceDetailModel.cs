namespace Contract.Models.Invoice
{
    public class InvoiceDetailModel
    {
        public InvoiceDetailModel(Guid invoiceId, Guid premiseId)
        {
            InvoiceDetailInvoiceId = invoiceId;
            InvoiceDetailPremiseId = premiseId;
        }

        public InvoiceDetailModel(
            int invoiceDetailId,
            Guid invoiceId,
            Guid premiseId,
            decimal rentalPrice,
            decimal electricityFee,
            decimal waterFee,
            decimal garbageFee)
        {
            InvoiceDetailId = invoiceDetailId;
            InvoiceDetailInvoiceId = invoiceId;
            InvoiceDetailPremiseId = premiseId;
            InvoiceDetailRentalPrice = rentalPrice;
            InvoiceDetailElectricityFee = electricityFee;
            InvoiceDetailWaterFee = waterFee;
            InvoiceDetailGarbageFee = garbageFee;
            InvoiceDetailTotalAmount = CalculateTotalAmount();
        }

        public InvoiceDetailModel() {}

        public int InvoiceDetailId { get; init; }

        public Guid InvoiceDetailInvoiceId { get; private set; }

        public Guid InvoiceDetailPremiseId { get; private set; }

        public decimal InvoiceDetailRentalPrice { get; private set; } = 0;

        public decimal InvoiceDetailElectricityFee { get; private set; } = 0;

        public decimal InvoiceDetailWaterFee { get; private set; } = 0;

        public decimal InvoiceDetailGarbageFee { get; private set; } = 0;

        public decimal InvoiceDetailTotalAmount { get; private set; } = 0;

        #region SET

        public void SetRentalPrice(decimal rentalPrice)
        {
            InvoiceDetailRentalPrice = rentalPrice;
        }
        
        public void SetElectricityFee(decimal electricityFee)
        {
            InvoiceDetailElectricityFee = electricityFee;
        }
        
        public void SetGarbageFee(decimal garbageFee)
        {
            InvoiceDetailGarbageFee = garbageFee;
        }

        public void SetWaterFee(decimal waterFee)
        {
            InvoiceDetailWaterFee = waterFee;
        }

        // public void SetTotalAmount(decimal? totalAmount)
        // {
        //     InvoiceDetailTotalAmount = totalAmount;
        // }

        private decimal CalculateTotalAmount()
        {
            return InvoiceDetailRentalPrice + InvoiceDetailElectricityFee + InvoiceDetailWaterFee + InvoiceDetailGarbageFee;
        }
        
        #endregion
    }
}