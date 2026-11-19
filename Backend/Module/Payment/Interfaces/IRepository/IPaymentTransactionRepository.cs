using Payment.Models.Payment;
using Payment.Utils.Enum;
using Shared.Interfaces;
using Shared.Persistence.Record;

namespace Payment.Interfaces.IRepository
{
    public interface IPaymentTransactionRepository
        : IBaseReadRepository<PaymentTransactionModel, Guid>, IBasePostRepository<PaymentTransactionModel>
    {
        Task<PaymentTransactionModel?> GetByTxnRefAsync(string txnRef,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PaymentTransactionModel>> GetByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PaymentTransactionModel>> GetByAccountIdAsync(Guid accountId,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingByAccountIdAsync(Guid? cursor,
            int pageSize, Guid accountId, CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingByStatusAsync(Guid? cursor,
            int pageSize, EPaymentStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Settles a transaction in a single conditional statement, so two IPNs arriving at the
        /// same time cannot both settle it and a later replay cannot overwrite an earlier result.
        /// <para>
        /// A row is settleable when it is still <see cref="EPaymentStatus.Pending"/>, or when it was
        /// swept to <see cref="EPaymentStatus.Expired"/> by this module's own timeout and the
        /// gateway now reports a success: the expiry is our guess, the signed success is a fact.
        /// </para>
        /// </summary>
        /// <returns>The number of rows updated. 0 means the transaction was already settled.</returns>
        Task<int> SettleByTxnRefAsync(string txnRef, PaymentSettlementModel settlement,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves abandoned <see cref="EPaymentStatus.Pending"/> transactions past their expiry into
        /// <see cref="EPaymentStatus.Expired"/>.
        /// </summary>
        /// <returns>The number of rows expired.</returns>
        Task<int> ExpireStalePendingAsync(DateTime utcNow, CancellationToken cancellationToken = default);
    }
}
