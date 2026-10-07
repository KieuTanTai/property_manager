using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Premise.Models.Business;
using Premise.Models.Product;

namespace Premise.Infrastructures.Persistence.Configuration
{
    public sealed class ContextProductBusinessTypeModelConfiguration : IEntityTypeConfiguration<ProductBusinessTypeModel>
    {
        public void Configure(EntityTypeBuilder<ProductBusinessTypeModel> entity)
        {
            entity.ToTable("product_business_type");

            entity.HasKey(productBusinessType => new
            {
                productBusinessType.ProductId,
                productBusinessType.BusinessTypeId
            });

            entity.Property(productBusinessType => productBusinessType.ProductId)
                .HasColumnName("pbt_product_id")
                .IsRequired();

            entity.Property(productBusinessType => productBusinessType.BusinessTypeId)
                .HasColumnName("pbt_business_type_id")
                .IsRequired();

            entity.Property(productBusinessType => productBusinessType.AssignedAt)
                .HasColumnName("pbt_assigned_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.HasOne<WhitelistProductModel>()
                .WithMany()
                .HasForeignKey(productBusinessType => productBusinessType.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<BusinessTypeModel>()
                .WithMany()
                .HasForeignKey(productBusinessType => productBusinessType.BusinessTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}