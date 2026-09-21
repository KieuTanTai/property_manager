using Contract.Models.Invoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contract.Infrastructure.Persistence.Configurations
{
    public sealed class ContextReceiptConfiguration : IEntityTypeConfiguration<ReceiptModel>
    {
        public void Configure(EntityTypeBuilder<ReceiptModel> entity)
        {
            entity.ToTable("receipt");
            entity.HasKey(receipt => receipt.ReceiptId);

            entity.Property(receipt => receipt.ReceiptId)
                .HasColumnName("receipt_id")
                .ValueGeneratedOnAdd();

            entity.Property(receipt => receipt.ReceiptInvoiceId)
                .HasColumnName("receipt_invoice_id")
                .IsRequired();

            entity.Property(receipt => receipt.ReceiptCreatedByAccountId)
                .HasColumnName("receipt_created_by_account_id")
                .IsRequired();

            entity.Property(receipt => receipt.ReceiptPaymentMethod)
                .HasColumnName("receipt_payment_method")
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasDefaultValueSql("'cash'")
                .IsRequired();

            entity.Property(receipt => receipt.ReceiptAmount)
                .HasColumnName("receipt_amount")
                .HasPrecision(18, 2)
                .HasDefaultValue(0.00)
                .IsRequired();

            entity.Property(receipt => receipt.ReceiptPaymentDate)
                .HasColumnName("receipt_payment_date")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(receipt => receipt.ReceiptTransactionReference)
                .HasColumnName("receipt_transaction_reference")
                .HasMaxLength(100);

            entity.Property(receipt => receipt.ReceiptGatewayTransactionNumber)
                .HasColumnName("receipt_gateway_transaction_number")
                .HasMaxLength(100);

            entity.Property(receipt => receipt.ReceiptTransactionInfo)
                .HasColumnName("receipt_transaction_info")
                .HasMaxLength(250);

            entity.Property(receipt => receipt.ReceiptBankName)
                .HasColumnName("receipt_bank_name")
                .HasMaxLength(100);

            entity.Property(receipt => receipt.ReceiptCreatedAt)
                .HasColumnName("receipt_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(receipt => receipt.ReceiptUpdatedAt)
                .HasColumnName("receipt_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasIndex(receipt => receipt.ReceiptInvoiceId)
                .IsUnique()
                .HasDatabaseName("uk_receipt_invoice_id");

            entity.HasIndex(receipt => receipt.ReceiptPaymentMethod)
                .HasDatabaseName("idx_receipt_payment_method");

            entity.HasIndex(receipt => receipt.ReceiptPaymentDate)
                .HasDatabaseName("idx_receipt_payment_date");

            entity.HasIndex(receipt => receipt.ReceiptTransactionReference)
                .HasDatabaseName("idx_receipt_transaction_reference");

            entity.HasIndex(receipt => receipt.ReceiptCreatedByAccountId)
                .HasDatabaseName("idx_receipt_created_by_account_id");

            entity.HasOne<MonthlyInvoiceModel>()
                .WithOne(invoice => invoice.InvoiceReceipt)
                .HasForeignKey<ReceiptModel>(receipt => receipt.ReceiptInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
