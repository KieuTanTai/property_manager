using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Premise.Models.Premise;

namespace Premise.Infrastructures.Persistence.Configuration
{
    public sealed class ContextPremiseMediaConfiguration : IEntityTypeConfiguration<PremiseMediaModel>
    {
        public void Configure(EntityTypeBuilder<PremiseMediaModel> entity)
        {
            entity.ToTable("premise_media");

            entity.HasKey(premiseMedia => premiseMedia.PremiseMediaId);

            entity.Property(premiseMedia => premiseMedia.PremiseMediaId)
                .HasColumnName("premise_media_id")
                .ValueGeneratedOnAdd();

            entity.Property(premiseMedia => premiseMedia.PremiseMediaPremiseId)
                .HasColumnName("premise_media_premise_id")
                .IsRequired();

            entity.Property(premiseMedia => premiseMedia.PremiseMediaImageUrl)
                .HasColumnName("premise_media_image_url")
                .HasConversion<string>()
                .IsRequired();

            entity.Property(premiseMedia => premiseMedia.PremiseMediaCreatedAt)
                .HasColumnName("premise_media_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(premiseMedia => premiseMedia.PremiseMediaUpdatedAt)
                .HasColumnName("premise_media_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne<PremiseModel>()
                .WithMany()
                .HasForeignKey(premiseMedia => premiseMedia.PremiseMediaPremiseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}