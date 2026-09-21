using Identity.Models.Account;
using Identity.Models.Permission;
using Identity.Models.Profile;
using Identity.Models.Role;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations
{
    public sealed class ContextAccountConfiguration : IEntityTypeConfiguration<AccountModel>
    {
        public void Configure(EntityTypeBuilder<AccountModel> entity)
        {
            entity.ToTable("account");

            entity.HasKey(account => account.AccountId);

            entity.Property(account => account.AccountId)
                .HasColumnName("account_id")
                .ValueGeneratedOnAdd();

            entity.Property(account => account.AccountEmail)
                .HasColumnName("account_email")
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(account => account.AccountEmail)
                .IsUnique()
                .HasDatabaseName("idx_account_email");

            entity.Property(account => account.AccountPassword)
                .HasColumnName("account_password")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(account => account.AccountIsActive)
                .HasColumnName("account_is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.HasIndex(account => new { account.AccountIsActive, account.AccountId })
                .HasDatabaseName("idx_account_is_active_account_id");

            entity.Property(account => account.AccountCreatedAt)
                .HasColumnName("account_created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.Property(account => account.AccountUpdatedAt)
                .HasColumnName("account_updated_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            entity.HasMany(account => account.Roles).WithMany().UsingEntity<AccountRoleModel>(
                right => right.HasOne<RoleModel>().WithMany().HasForeignKey(role => role.RoleId)
                    .OnDelete(DeleteBehavior.Restrict),
                left => left.HasOne<AccountModel>().WithMany().HasForeignKey(account => account.AccountId)
                    .OnDelete(DeleteBehavior.Restrict),
                join => {
                    join.ToTable("account_role");
                    join.HasKey(accountRole => new
                    {
                        accountRole.AccountId,
                        accountRole.RoleId
                    });

                    join.Property(accountRole => accountRole.AccountId)
                        .HasColumnName("account_id");

                    join.Property(accountRole => accountRole.RoleId)
                        .HasColumnName("role_id");

                    join.Property(accountRole => accountRole.AssignedAt)
                        .HasColumnName("assigned_at")
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .ValueGeneratedOnAdd();
                });

            entity.HasMany(account => account.AdditionalPermissions).WithMany().UsingEntity<AccountAdditionalPermissionModel>(
                right => right.HasOne<PermissionModel>().WithMany().HasForeignKey(permission => permission.PermissionId)
                    .OnDelete(DeleteBehavior.Restrict),
                left => left.HasOne<AccountModel>().WithMany().HasForeignKey(account => account.AccountId)
                    .OnDelete(DeleteBehavior.Restrict),
                join => {
                    join.ToTable("account_additional_permission");
                    join.HasKey(accountPermission => new
                    {
                        accountPermission.AccountId,
                        accountPermission.PermissionId
                    });

                    join.Property(accountPermission => accountPermission.AccountId)
                        .HasColumnName("account_id");

                    join.Property(accountPermission => accountPermission.PermissionId)
                        .HasColumnName("permission_id");

                    join.Property(accountPermission => accountPermission.AssignedAt)
                        .HasColumnName("assigned_at")
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .ValueGeneratedOnAdd();
                });

            entity.HasOne(account => account.UserProfile)
                .WithOne()
                .HasForeignKey<UserProfileModel>(profile => profile.UserProfileAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}