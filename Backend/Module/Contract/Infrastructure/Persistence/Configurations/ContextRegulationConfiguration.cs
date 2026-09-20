using Contract.Models.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySql.EntityFrameworkCore.Extensions;

namespace Contract.Infrastructure.Persistence.Configurations
{
    public sealed class ContextRegulationConfiguration : IEntityTypeConfiguration<RegulationModel>
    {
        public void Configure(EntityTypeBuilder<RegulationModel> entity)
        {
            entity.ToTable("regulation");
            entity.HasKey(regulation => regulation.RegulationId);

            entity.Property(regulation => regulation.RegulationId)
                .HasColumnName("regulation_id")
                .ValueGeneratedOnAdd();

            entity.Property(regulation => regulation.RegulationName)
                .HasColumnName("regulation_name")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(regulation => regulation.RegulationDescription)
                .HasColumnName("regulation_description")
                .HasMaxLength(255);

            entity.Property(regulation => regulation.RegulationFineAmount)
                .HasColumnName("regulation_fine_amount")
                .HasPrecision(18, 2);

            entity.Property(regulation => regulation.RegulationIsActive)
                .HasColumnName("regulation_is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(regulation => regulation.RegulationCreatedAt)
                .HasColumnName("regulation_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(regulation => regulation.RegulationUpdatedAt)
                .HasColumnName("regulation_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasIndex(regulation => regulation.RegulationName)
                .HasDatabaseName("idx_regulation_name")
                .HasPrefixLength(20);
        }
    }
}
