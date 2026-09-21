using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Premise.Models.Premise;

namespace Premise.Infrastructures.Persistence.Configuration
{
    public sealed class ContextRentedPremiseConfiguration : IEntityTypeConfiguration<RentedPremiseModel>
    {
        public void Configure(EntityTypeBuilder<RentedPremiseModel> entity)
        {
            entity.ToTable("rented_premise");

            entity.HasKey(rentedPremise => new
            {
                rentedPremise.ContractId,
                rentedPremise.PremiseId
            });

            entity.Property(rentedPremise => rentedPremise.ContractId)
                .HasColumnName("rented_premise_contract_id")
                .IsRequired();

            entity.Property(rentedPremise => rentedPremise.PremiseId)
                .HasColumnName("rented_premise_premise_id")
                .IsRequired();

            entity.HasOne<PremiseModel>()
                .WithMany()
                .HasForeignKey(rentedPremise => rentedPremise.PremiseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}