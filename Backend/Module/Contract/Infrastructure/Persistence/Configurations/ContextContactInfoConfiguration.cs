using Contract.Models.Contact;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contract.Infrastructure.Persistence.Configurations
{
    public sealed class ContextContactInfoConfiguration : IEntityTypeConfiguration<ContactInfoModel>
    {
        public void Configure(EntityTypeBuilder<ContactInfoModel> entity)
        {
            entity.ToTable("contact_info");
            entity.HasKey(contactInfo => contactInfo.ContactInfoId);

            entity.Property(contactInfo => contactInfo.ContactInfoId)
                .HasColumnName("contact_info_id")
                .ValueGeneratedOnAdd();

            entity.Property(contactInfo => contactInfo.ContactInfoEmail)
                .HasColumnName("contact_info_email")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(contactInfo => contactInfo.ContactInfoPhone)
                .HasColumnName("contact_info_phone")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(contactInfo => contactInfo.ContactInfoAddress)
                .HasColumnName("contact_info_address")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(contactInfo => contactInfo.ContactSocialLink)
                .HasColumnName("contact_social_link")
                .HasMaxLength(255)
                .IsRequired(false);

            entity.Property(contactInfo => contactInfo.ContactInfoCreatedAt)
                .HasColumnName("contact_info_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(contactInfo => contactInfo.ContactInfoUpdatedAt)
                .HasColumnName("contact_info_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasIndex(contactInfo => contactInfo.ContactInfoEmail)
                .HasDatabaseName("idx_contact_info_email")
                .IsUnique();
            entity.HasIndex(contactInfo => contactInfo.ContactInfoPhone)
                .HasDatabaseName("idx_contact_info_phone")
                .IsUnique();
        }
    }
}
