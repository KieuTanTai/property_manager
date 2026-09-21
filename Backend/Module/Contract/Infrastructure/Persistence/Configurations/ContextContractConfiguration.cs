using Contract.Models.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contract.Infrastructure.Persistence.Configurations
{
    public sealed class ContextContractConfiguration : IEntityTypeConfiguration<ContractModel>
    {
        public void Configure(EntityTypeBuilder<ContractModel> entity)
        {
            entity.ToTable("contract");
            entity.HasKey(contract => contract.ContractId);

            entity.Property(contract => contract.ContractId)
                .HasColumnName("contract_id")
                .ValueGeneratedOnAdd();

            entity.Property(contract => contract.ContractAccountId)
                .HasColumnName("contract_account_id")
                .IsRequired();

            entity.Property(contract => contract.ContractDeposit)
                .HasColumnName("contract_deposit")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(contract => contract.ContractRentalPrice)
                .HasColumnName("contract_rental_price")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(contract => contract.ContractPremiseReturnDate)
                .HasColumnName("contract_premise_return_date")
                .HasColumnType("timestamp")
                .IsRequired();

            entity.Property(contract => contract.ContractStatus)
                .HasColumnName("contract_status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending_approval'")
                .IsRequired();

            entity.Property(contract => contract.ContractTerminationDate)
                .HasColumnName("contract_termination_date")
                .HasColumnType("timestamp");

            entity.Property(contract => contract.ContractCreatedAt)
                .HasColumnName("contract_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(contract => contract.ContractUpdatedAt)
                .HasColumnName("contract_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasMany(contract => contract.ContractViolations)
                .WithOne()
                .HasForeignKey(violation => violation.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(contract => contract.ContractInvoices)
                .WithOne()
                .HasForeignKey(invoice => invoice.InvoiceContractId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(contract => contract.ContractRegulations)
                .WithMany()
                .UsingEntity<ContractRegulationModel>(
                    right => right.HasOne<RegulationModel>()
                        .WithMany()
                        .HasForeignKey(join => join.RegulationId)
                        .OnDelete(DeleteBehavior.Restrict),
                    left => left.HasOne<ContractModel>()
                        .WithMany()
                        .HasForeignKey(join => join.ContractId)
                        .OnDelete(DeleteBehavior.Restrict),
                    join => {
                        join.ToTable("contract_regulation");

                        join.HasKey(item => new { item.RegulationId, item.ContractId });

                        join.Property(item => item.RegulationId)
                            .HasColumnName("regulation_id");
                        join.Property(item => item.ContractId)
                            .HasColumnName("contract_id");
                        join.Property(item => item.AssignedAt)
                            .HasColumnName("assigned_at")
                            .HasColumnType("timestamp")
                            .HasDefaultValueSql("CURRENT_TIMESTAMP")
                            .ValueGeneratedOnAdd();
                    });
        }
    }
}