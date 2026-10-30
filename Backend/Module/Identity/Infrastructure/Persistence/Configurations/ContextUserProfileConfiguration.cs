using Identity.Models.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations
{
    public sealed class ContextUserProfileConfiguration : IEntityTypeConfiguration<UserProfileModel>
    {
        public void Configure(EntityTypeBuilder<UserProfileModel> entity)
        {
            // Configuration logic for UserProfileModel
            entity.ToTable("user_profile");
            entity.HasKey(userProfile => userProfile.UserProfileId);

            entity.Property(userProfile => userProfile.UserProfileId)
                .HasColumnName("user_profile_id");

            entity.Property(userProfile => userProfile.UserProfileAccountId)
                .HasColumnName("user_profile_account_id")
                .IsRequired();

            entity.HasIndex(userProfile => userProfile.UserProfileAccountId)
                .IsUnique()
                .HasDatabaseName("idx_user_profile_account_id");

            entity.Property(userProfile => userProfile.UserProfileFirstName)
                .HasColumnName("user_profile_first_name")
                .HasMaxLength(30);

            entity.Property(userProfile => userProfile.UserProfileLastName)
                .HasColumnName("user_profile_last_name")
                .HasMaxLength(30);

            entity.Property(userProfile => userProfile.UserProfileDateOfBirth)
                .HasColumnName("user_profile_date_of_birth");

            entity.HasIndex(userProfile => userProfile.UserProfileDateOfBirth)
                .HasDatabaseName("idx_user_profile_date_of_birth");

            entity.Property(userProfile => userProfile.UserProfileGender)
                .HasColumnName("user_profile_gender")
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(userProfile => userProfile.UserProfilePhoneNumber)
                .HasColumnName("user_profile_phone_number")
                .HasMaxLength(10);

            entity.HasIndex(userProfile => userProfile.UserProfilePhoneNumber)
                .HasDatabaseName("idx_user_profile_phone_number");

            entity.Property(userProfile => userProfile.UserProfileAvatarUrl)
                .HasColumnName("user_profile_avatar_url")
                .HasColumnType("longtext");

            entity.Property(model => model.UserProfileAddress)
                .HasColumnName("user_profile_address")
                .HasMaxLength(255);

            entity.Property(userProfile => userProfile.UserProfileCreatedAt)
                .HasColumnName("user_profile_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(userProfile => userProfile.UserProfileUpdatedAt)
                .HasColumnName("user_profile_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
        }
    }
}
