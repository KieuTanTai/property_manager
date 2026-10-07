using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Premise.Models.Business;
using Premise.Models.Premise;
using Premise.Models.Product;
using MySqlModelBuilderExtensions=
    MySql.EntityFrameworkCore.Extensions.MySQLModelBuilderExtensions;

namespace Premise.Infrastructures.Persistence.DbContext
{
    public sealed class PremiseDbContext(DbContextOptions<PremiseDbContext> contextOptions)
        : Microsoft.EntityFrameworkCore.DbContext(contextOptions)
    {
        public DbSet<PremiseModel> Premises { get; set; }
        public DbSet<LocationModel> Locations { get; set; }
        public DbSet<PremiseBusinessTypeModel> PremiseBusinessTypes { get; set; }
        public DbSet<PremiseMediaModel> PremiseMedias { get; set; }
        public DbSet<ProductBusinessTypeModel> ProductBusinessTypes { get; set; }
        public DbSet<WhitelistProductModel> WhitelistProducts { get; set; }
        public DbSet<RentedPremiseModel> RentedPremiseModels { get; set; }
        public DbSet<BusinessTypeModel> BusinessTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasCharSet("utf8mb4");

            MySqlModelBuilderExtensions.UseCollation(
                modelBuilder,
                "utf8mb4_unicode_ci"
            );

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(PremiseDbContext).Assembly
            );
        }
    }
}