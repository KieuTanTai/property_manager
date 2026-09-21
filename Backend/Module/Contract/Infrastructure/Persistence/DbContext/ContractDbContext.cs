using Contract.Models.Contract;
using Contract.Models.Invoice;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using MySqlModelBuilderExtensions=
    MySql.EntityFrameworkCore.Extensions.MySQLModelBuilderExtensions;

namespace Contract.Infrastructure.Persistence.DbContext
{
    public sealed class ContractDbContext(DbContextOptions<ContractDbContext> options)
        : Microsoft.EntityFrameworkCore.DbContext(options)
    {
        public DbSet<ContractModel> Contracts { get; set; }
        public DbSet<ContractRegulationModel> ContractRegulations { get; set; }
        public DbSet<ContractViolationModel> ContractViolations { get; set; }
        public DbSet<RegulationModel> Regulations { get; set; }
        public DbSet<MonthlyInvoiceModel> MonthlyInvoices { get; set; }
        public DbSet<InvoiceDetailModel> InvoiceDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasCharSet("utf8mb4");
            MySqlModelBuilderExtensions.UseCollation(modelBuilder, "utf8mb4_unicode_ci");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContractDbContext).Assembly);
        }
    }
}