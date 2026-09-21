using Contract.Models.Invoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contract.Infrastructure.Persistence.Configurations
{
    public sealed class ContextMonthlyInvoiceConfiguration : IEntityTypeConfiguration<MonthlyInvoiceModel>
    {
        public void Configure(EntityTypeBuilder<MonthlyInvoiceModel> entity)
        {
            entity.ToTable("monthly_invoice");
            entity.HasKey(invoice => invoice.InvoiceId);

            entity.Property(invoice => invoice.InvoiceId)
                .HasColumnName("invoice_id")
                .ValueGeneratedOnAdd();

            entity.Property(invoice => invoice.InvoiceContractId)
                .HasColumnName("invoice_contract_id")
                .IsRequired();

            entity.Property(invoice => invoice.InvoicePaymentDate)
                .HasColumnName("invoice_payment_date")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(invoice => invoice.InvoiceDueDate)
                .HasColumnName("invoice_due_date")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("(CURRENT_TIMESTAMP + INTERVAL 30 DAY)")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(invoice => invoice.InvoiceTotalAmount)
                .HasColumnName("invoice_total_amount")
                .HasPrecision(18, 2);

            entity.Property(invoice => invoice.InvoiceStatus)
                .HasColumnName("invoice_status")
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasDefaultValueSql("'unpaid'")
                .IsRequired();

            entity.Property(invoice => invoice.InvoiceCreatedAt)
                .HasColumnName("invoice_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(invoice => invoice.InvoiceUpdatedAt)
                .HasColumnName("invoice_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasIndex(invoice => invoice.InvoicePaymentDate)
                .HasDatabaseName("idx_invoice_payment_date");

            entity.HasIndex(invoice => invoice.InvoiceDueDate)
                .HasDatabaseName("idx_invoice_due_date");

            entity.HasMany(invoice => invoice.InvoiceDetails)
                .WithOne()
                .HasForeignKey(detail => detail.InvoiceDetailInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}