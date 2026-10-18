namespace Payment.Utils.Enum
{
    public enum EPaymentStatus
    {
        Pending,

        Succeeded,

        Failed,

        Expired,

        /// <summary>
        /// VNPAY reported an outcome where the customer may already have been debited without the
        /// transaction succeeding ("vnp_ResponseCode" 07, "vnp_TransactionStatus" 04). It is
        /// terminal - no further IPN can settle it - but it must not be mistaken for a clean
        /// failure, so it is kept apart and needs reconciliation.
        /// </summary>
        NeedsReview
    }
}
