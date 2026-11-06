using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Premise.Models.Business;
using Premise.Models.Premise;

namespace Premise.Infrastructure.Persistence.Configuration
{
    public sealed class ContextPremiseBusinessTypeConfiguration : IEntityTypeConfiguration<PremiseBusinessTypeModel>
    {
        public void Configure(EntityTypeBuilder<PremiseBusinessTypeModel> entity)
        {
            entity.ToTable("premise_business_type");

            entity.HasKey(premiseBusinessType => new
            {
                premiseBusinessType.PremiseId,
                premiseBusinessType.BusinessTypeId
            });

            entity.Property(premiseBusinessType => premiseBusinessType.PremiseId)
                .HasColumnName("pre_bt_premise_id")
                .IsRequired();

            entity.Property(premiseBusinessType => premiseBusinessType.BusinessTypeId)
                .HasColumnName("pre_bt_business_type_id")
                .IsRequired();

            entity.Property(premiseBusinessType => premiseBusinessType.AssignedAt)
                .HasColumnName("pre_bt_asigned_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne<PremiseModel>()
                .WithMany()
                .HasForeignKey(premiseBusinessType => premiseBusinessType.PremiseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<BusinessTypeModel>()
                .WithMany()
                .HasForeignKey(premiseBusinessType => premiseBusinessType.BusinessTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}