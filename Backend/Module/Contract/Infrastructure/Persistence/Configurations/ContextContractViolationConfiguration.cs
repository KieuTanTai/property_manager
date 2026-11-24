using Contract.Models.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySql.EntityFrameworkCore.Extensions;

namespace Contract.Infrastructure.Persistence.Configurations
{
    public sealed class ContextContractViolationConfiguration : IEntityTypeConfiguration<ContractViolationModel>
    {
        public void Configure(EntityTypeBuilder<ContractViolationModel> entity)
        {
            entity.ToTable("contract_violation");
            entity.HasKey(violation => violation.ViolationId);

            entity.Property(violation => violation.ViolationId)
                .HasColumnName("contract_violation_id")
                .ValueGeneratedOnAdd();

            entity.Property(violation => violation.ContractId)
                .HasColumnName("contract_id")
                .IsRequired();

            entity.Property(violation => violation.ViolationContent)
                .HasColumnName("violation_content")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(violation => violation.ViolationPenaltyAmount)
                .HasColumnName("violation_penalty_amount")
                .HasPrecision(18, 2);

            entity.Property(violation => violation.ViolationDate)
                .HasColumnName("violation_date")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(violation => violation.ViolationDueDate)
                .HasColumnName("violation_due_date")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("(CURRENT_TIMESTAMP + INTERVAL 7 DAY)")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(violation => violation.ViolationIsResolved)
                .HasColumnName("violation_is_resolved")
                .HasDefaultValue(false)
                .IsRequired();

            entity.HasIndex(violation => violation.ViolationContent)
                .HasDatabaseName("idx_contract_violation_content")
                .HasPrefixLength(20);

            entity.HasIndex(violation => violation.ViolationDate)
                .HasDatabaseName("idx_contract_violation_date");
        }
    }
}