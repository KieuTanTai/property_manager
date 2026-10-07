using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySql.EntityFrameworkCore.Extensions;
using Premise.Models.Business;
using Premise.Models.Premise;
using Premise.Models.Product;

namespace Premise.Infrastructures.Persistence.Configuration
{
    public sealed class ContextBusinessTypeConfiguration : IEntityTypeConfiguration<BusinessTypeModel>
    {
        public void Configure(EntityTypeBuilder<BusinessTypeModel> entity)
        {
            entity.ToTable("business_type");

            entity.HasKey(businessType => businessType.BusinessTypeId);

            entity.Property(businessType => businessType.BusinessTypeId)
                .HasColumnName("business_type_id")
                .ValueGeneratedOnAdd();

            entity.Property(businessType => businessType.BusinessTypeName)
                .HasColumnName("business_type_name")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(businessType => businessType.BusinessTypeDescription)
                .HasColumnName("business_type_description")
                .HasMaxLength(255);

            entity.Property(businessType => businessType.BusinessTypeIsActive)
                .HasColumnName("business_type_is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(businessType => businessType.BusinessTypeCreatedAt)
                .HasColumnName("business_type_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(businessType => businessType.BusinessTypeUpdatedAt)
                .HasColumnName("business_type_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasIndex(businessType => businessType.BusinessTypeName)
                .IsUnique()
                .HasDatabaseName("idx_business_type_name")
                .HasPrefixLength(20);

            entity.HasMany(businessType => businessType.Premises)
                .WithMany()
                .UsingEntity<PremiseBusinessTypeModel>(
                    right => right.HasOne<PremiseModel>().WithMany().HasForeignKey(premise => premise.PremiseId)
                        .OnDelete(DeleteBehavior.Restrict),
                    left => left.HasOne<BusinessTypeModel>().WithMany().HasForeignKey(businessType => businessType.BusinessTypeId)
                        .OnDelete(DeleteBehavior.Restrict),
                    join => {
                        join.ToTable("premise_business_type");
                        join.HasKey(premiseBusinessType => new
                        {
                            premiseBusinessType.PremiseId,
                            premiseBusinessType.BusinessTypeId
                        });

                        join.Property(premiseBusinessType => premiseBusinessType.PremiseId)
                            .HasColumnName("pre_bt_premise_id")
                            .IsRequired();

                        join.Property(premiseBusinessType => premiseBusinessType.BusinessTypeId)
                            .HasColumnName("pre_bt_business_type_id")
                            .IsRequired();

                        join.Property(premiseBusinessType => premiseBusinessType.AssignedAt)
                            .HasColumnName("pre_bt_assigned_at")
                            .HasColumnType("timestamp")
                            .HasDefaultValueSql("CURRENT_TIMESTAMP")
                            .ValueGeneratedOnAdd();
                    }
                );

            entity.HasMany(businessType => businessType.WhitelistProducts)
                .WithMany()
                .UsingEntity<ProductBusinessTypeModel>(
                    right => right.HasOne<WhitelistProductModel>().WithMany().HasForeignKey(whitelistProduct => whitelistProduct.ProductId)
                        .OnDelete(DeleteBehavior.Restrict),
                    left => left.HasOne<BusinessTypeModel>().WithMany().HasForeignKey(businessType => businessType.BusinessTypeId)
                        .OnDelete(DeleteBehavior.Restrict),
                    join => {
                        join.ToTable("product_business_type");
                        join.HasKey(businessTypeWhitelistProduct => new
                        {
                            businessTypeWhitelistProduct.ProductId,
                            businessTypeWhitelistProduct.BusinessTypeId
                        });

                        join.Property(businessTypeWhitelistProduct => businessTypeWhitelistProduct.ProductId)
                            .HasColumnName("pbt_product_id")
                            .IsRequired();

                        join.Property(businessTypeWhitelistProduct => businessTypeWhitelistProduct.BusinessTypeId)
                            .HasColumnName("pbt_business_type_id")
                            .IsRequired();

                        join.Property(businessTypeWhitelistProduct => businessTypeWhitelistProduct.AssignedAt)
                            .HasColumnName("pbt_assigned_at")
                            .HasColumnType("timestamp")
                            .HasDefaultValueSql("CURRENT_TIMESTAMP")
                            .ValueGeneratedOnAdd();
                    }
                );
        }
    }
}