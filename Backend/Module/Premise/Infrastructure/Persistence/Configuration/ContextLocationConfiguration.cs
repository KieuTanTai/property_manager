using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Premise.Models.Premise;

namespace Premise.Infrastructures.Persistence.Configuration
{
    public sealed class ContextLocationConfiguration : IEntityTypeConfiguration<LocationModel>
    {
        public void Configure(EntityTypeBuilder<LocationModel> entity)
        {
            entity.ToTable("location");

            entity.HasKey(location => location.LocationId);

            entity.Property(location => location.LocationId)
                .HasColumnName("location_id")
                .ValueGeneratedOnAdd();

            entity.Property(location => location.LocationAddress)
                .HasColumnName("location_address")
                .HasConversion<string>()
                .IsRequired();

            entity.Property(location => location.LocationCreatedAt)
                .HasColumnName("location_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(location => location.LocationUpdatedAt)
                .HasColumnName("location_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
        }
    }
}