using Microsoft.AspNetCore.Http;
using Payment.Models.Payment;
using Payment.Utils.Enum;
using Shared.Persistence.Record;

namespace Payment.Interfaces.IApplication
{
    public interface IPaymentApplication
    {
        Task<PaymentUrlResult> CreatePaymentUrlAsync(Guid invoiceId, Guid accountId, decimal amount,
            string? description, string? bankCode, string? language, string ipAddress,
            CancellationToken cancellationToken = default);

        Task<IpnResult> ProcessIpnAsync(IQueryCollection query,
            CancellationToken cancellationToken = default);

        Task<PaymentTransactionModel?> GetReturnResultAsync(IQueryCollection query,
            CancellationToken cancellationToken = default);

        Task<PaymentTransactionModel?> GetByIdAsync(Guid paymentTransactionId,
            CancellationToken cancellationToken = default);

        Task<PaymentTransactionModel?> GetByTxnRefAsync(string txnRef,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PaymentTransactionModel>> GetByInvoiceIdAsync(Guid invoiceId,
            CancellationToken cancellationToken = default);

        Task<PaymentTransactionModel?> GetInvoicePaymentStatusAsync(Guid invoiceId,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingByAccountIdAsync(Guid? cursor,
            int pageSize, Guid accountId, CancellationToken cancellationToken = default);

        Task<RecordBaseCursorPage<PaymentTransactionModel>> GetApplyPagingByStatusAsync(Guid? cursor,
            int pageSize, EPaymentStatus status, CancellationToken cancellationToken = default);
    }
}
