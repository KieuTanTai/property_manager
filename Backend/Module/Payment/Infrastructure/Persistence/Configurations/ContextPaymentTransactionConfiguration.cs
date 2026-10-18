using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Models.Payment;

namespace Payment.Infrastructure.Persistence.Configurations
{
    public sealed class ContextPaymentTransactionConfiguration
        : IEntityTypeConfiguration<PaymentTransactionModel>
    {
        public void Configure(EntityTypeBuilder<PaymentTransactionModel> entity)
        {
            entity.ToTable("payment_transaction");
            entity.HasKey(transaction => transaction.PaymentTransactionId);

            entity.Property(transaction => transaction.PaymentTransactionId)
                .HasColumnName("payment_transaction_id")
                .ValueGeneratedOnAdd();

            entity.Property(transaction => transaction.PaymentInvoiceId)
                .HasColumnName("payment_invoice_id")
                .IsRequired();

            entity.Property(transaction => transaction.PaymentAccountId)
                .HasColumnName("payment_account_id")
                .IsRequired();

            entity.Property(transaction => transaction.PaymentAmount)
                .HasColumnName("payment_amount")
                .HasPrecision(18, 2)
                .HasDefaultValue(0.00)
                .IsRequired();

            entity.Property(transaction => transaction.PaymentStatus)
                .HasColumnName("payment_status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(transaction => transaction.PaymentCurrency)
                .HasColumnName("payment_currency")
                .HasMaxLength(3)
                .HasDefaultValue("VND")
                .IsRequired();

            entity.Property(transaction => transaction.PaymentTxnRef)
                .HasColumnName("payment_txn_ref")
                .HasMaxLength(50);

            entity.Property(transaction => transaction.PaymentGatewayTransactionNumber)
                .HasColumnName("payment_gateway_transaction_number")
                .HasMaxLength(100);

            entity.Property(transaction => transaction.PaymentBankCode)
                .HasColumnName("payment_bank_code")
                .HasMaxLength(20);

            entity.Property(transaction => transaction.PaymentBankTranNo)
                .HasColumnName("payment_bank_tran_no")
                .HasMaxLength(100);

            entity.Property(transaction => transaction.PaymentResponseCode)
                .HasColumnName("payment_response_code")
                .HasMaxLength(10);

            entity.Property(transaction => transaction.PaymentTransactionStatusCode)
                .HasColumnName("payment_transaction_status_code")
                .HasMaxLength(10);

            entity.Property(transaction => transaction.PaymentCardType)
                .HasColumnName("payment_card_type")
                .HasMaxLength(50);

            entity.Property(transaction => transaction.PaymentOrderInfo)
                .HasColumnName("payment_order_info")
                .HasMaxLength(255);

            entity.Property(transaction => transaction.PaymentPayDate)
                .HasColumnName("payment_pay_date")
                .HasColumnType("timestamp");

            entity.Property(transaction => transaction.PaymentExpiresAt)
                .HasColumnName("payment_expires_at")
                .HasColumnType("timestamp")
                .IsRequired();

            entity.Property(transaction => transaction.PaymentCreatedAt)
                .HasColumnName("payment_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(transaction => transaction.PaymentUpdatedAt)
                .HasColumnName("payment_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasIndex(transaction => transaction.PaymentTxnRef)
                .IsUnique()
                .HasDatabaseName("uk_payment_txn_ref");

            entity.HasIndex(transaction => transaction.PaymentInvoiceId)
                .HasDatabaseName("idx_payment_invoice_id");

            entity.HasIndex(transaction => transaction.PaymentAccountId)
                .HasDatabaseName("idx_payment_account_id");

            entity.HasIndex(transaction => transaction.PaymentStatus)
                .HasDatabaseName("idx_payment_status");

            entity.HasIndex(transaction => transaction.PaymentCreatedAt)
                .HasDatabaseName("idx_payment_created_at");
        }
    }
}
