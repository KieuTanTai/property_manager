using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Premise.Models.Business;
using Premise.Models.Premise;

namespace Premise.Infrastructures.Persistence.Configuration
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
                .HasColumnName("premise_id")
                .IsRequired();

            entity.Property(premiseBusinessType => premiseBusinessType.BusinessTypeId)
                .HasColumnName("business_type_id")
                .IsRequired();

            entity.Property(premiseBusinessType => premiseBusinessType.AssignedAt)
                .HasColumnName("assigned_at")
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