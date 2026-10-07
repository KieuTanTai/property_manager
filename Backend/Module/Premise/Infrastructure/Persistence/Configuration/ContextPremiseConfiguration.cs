using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySql.EntityFrameworkCore.Extensions;
using Premise.Models.Business;
using Premise.Models.Premise;

namespace Premise.Infrastructures.Persistence.Configuration
{
    public sealed class ContextPremiseConfiguration : IEntityTypeConfiguration<PremiseModel>
    {
        public void Configure(EntityTypeBuilder<PremiseModel> entity)
        {
            entity.ToTable("premise");

            entity.HasKey(premise => premise.PremiseId);

            entity.Property(premise => premise.PremiseId)
                .HasColumnName("premise_id")
                .ValueGeneratedOnAdd();

            entity.Property(premise => premise.PremiseName)
                .HasColumnName("premise_name")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(premise => premise.PremiseStatus)
                .HasColumnName("premise_status")
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(premise => premise.PremisePosition)
                .HasColumnName("premise_position")
                .HasConversion<int>()
                .IsRequired();

            entity.Property(premise => premise.PremiseFloor)
                .HasColumnName("premise_floor")
                .HasConversion<int>()
                .IsRequired();

            entity.Property(premise => premise.PremiseArea)
                .HasColumnName("premise_area")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(premise => premise.PremiseDescription)
                .HasColumnName("premise_description")
                .HasMaxLength(255);

            entity.Property(premise => premise.PremiseCreatedAt)
                .HasColumnName("premise_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(premise => premise.PremiseUpdatedAt)
                .HasColumnName("premise_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasIndex(premise => premise.PremiseName)
                .HasPrefixLength(20);

            entity.HasOne(premise => premise.PremiseLocation)
                .WithOne()
                .HasForeignKey<PremiseModel>(premise => premise.PremiseLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(premise => premise.PremiseMedia)
                .WithOne()
                .HasForeignKey(premiseMedia => premiseMedia.PremiseMediaPremiseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(premise => premise.PremiseBusinessTypes)
                .WithMany()
                .UsingEntity<PremiseBusinessTypeModel>(
                    right => right.HasOne<BusinessTypeModel>().WithMany().HasForeignKey(businessType => businessType.BusinessTypeId)
                        .OnDelete(DeleteBehavior.Restrict),
                    left => left.HasOne<PremiseModel>().WithMany().HasForeignKey(premise => premise.PremiseId)
                        .OnDelete(DeleteBehavior.Restrict),
                    join => {
                        join.ToTable("premise_business_type");
                        join.HasKey(premiseBusinessType => new
                        {
                            premiseBusinessType.PremiseId,
                            premiseBusinessType.BusinessTypeId
                        });

                        join.Property(premiseBusinessType => premiseBusinessType.PremiseId)
                            .HasColumnName("premise_id");

                        join.Property(premiseBusinessType => premiseBusinessType.BusinessTypeId)
                            .HasColumnName("business_type_id");

                        join.Property(premiseBusinessType => premiseBusinessType.AssignedAt)
                            .HasColumnName("assigned_at")
                            .HasColumnType("timestamp")
                            .HasDefaultValueSql("CURRENT_TIMESTAMP")
                            .ValueGeneratedOnAdd();
                    });
        }
    }
}