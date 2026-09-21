using Contract.Models.Invoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contract.Infrastructure.Persistence.Configurations
{
    public sealed class ContextInvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetailModel>
    {
        public void Configure(EntityTypeBuilder<InvoiceDetailModel> entity)
        {
            entity.ToTable("invoice_detail");
            entity.HasKey(detail => detail.InvoiceDetailId);

            entity.Property(detail => detail.InvoiceDetailId)
                .HasColumnName("invoice_detail_id")
                .ValueGeneratedOnAdd();

            entity.Property(detail => detail.InvoiceDetailInvoiceId)
                .HasColumnName("invoice_detail_invoice_id")
                .IsRequired();

            entity.Property(detail => detail.InvoiceDetailPremiseId)
                .HasColumnName("invoice_detail_premise_id")
                .IsRequired();

            entity.Property(detail => detail.InvoiceDetailRentalPrice)
                .HasColumnName("invoice_detail_rental_price")
                .HasPrecision(18, 2)
                .HasDefaultValue(0.00)
                .IsRequired();

            entity.Property(detail => detail.InvoiceDetailElectricityFee)
                .HasColumnName("invoice_detail_electricity_fee")
                .HasPrecision(18, 2)
                .HasDefaultValue(0.00)
                .IsRequired();

            entity.Property(detail => detail.InvoiceDetailWaterFee)
                .HasColumnName("invoice_detail_water_fee")
                .HasPrecision(18, 2)
                .HasDefaultValue(0.00)
                .IsRequired();

            entity.Property(detail => detail.InvoiceDetailGarbageFee)
                .HasColumnName("invoice_detail_garbage_fee")
                .HasPrecision(18, 2)
                .HasDefaultValue(0.00)
                .IsRequired();

            entity.Property(detail => detail.InvoiceDetailTotalAmount)
                .HasColumnName("invoice_detail_total_amount")
                .HasPrecision(18, 2)
                .HasComputedColumnSql("""
                                      invoice_detail_rental_price +
                                      invoice_detail_electricity_fee +
                                      invoice_detail_water_fee +
                                      invoice_detail_garbage_fee
                                      """, true)
                .IsRequired();
        }
    }
}