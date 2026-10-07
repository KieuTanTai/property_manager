using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Premise.Models.Product;

namespace Premise.Infrastructures.Persistence.Configuration
{
    public sealed class ContextWhitelistProductConfiguration : IEntityTypeConfiguration<WhitelistProductModel>
    {
        public void Configure(EntityTypeBuilder<WhitelistProductModel> entity)
        {
            entity.ToTable("whitelist_product");

            entity.HasKey(whitelistProduct => whitelistProduct.WhitelistProductId);

            entity.Property(whitelistProduct => whitelistProduct.WhitelistProductId)
                .HasColumnName("whitelist_product_id")
                .ValueGeneratedOnAdd();

            entity.Property(whitelistProduct => whitelistProduct.WhitelistProductName)
                .HasColumnName("whitelist_product_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(whitelistProduct => whitelistProduct.WhitelistProductDescription)
                .HasColumnName("whitelist_product_description")
                .HasMaxLength(255);

            entity.Property(whitelistProduct => whitelistProduct.WhitelistProductCreatedAt)
                .HasColumnName("whitelist_product_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(whitelistProduct => whitelistProduct.WhitelistProductUpdatedAt)
                .HasColumnName("whitelist_product_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasIndex(whitelistProduct => whitelistProduct.WhitelistProductName)
                .HasDatabaseName("idx_whitelist_product_name");
        }
    }
}