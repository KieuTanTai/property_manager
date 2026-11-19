using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Payment.Models.Payment;
using MySqlModelBuilderExtensions=
    MySql.EntityFrameworkCore.Extensions.MySQLModelBuilderExtensions;

namespace Payment.Infrastructure.Persistence.DbContext
{
    public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : Microsoft.EntityFrameworkCore.DbContext(options)
    {
        public DbSet<PaymentTransactionModel> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasCharSet("utf8mb4");
            MySqlModelBuilderExtensions.UseCollation(modelBuilder, "utf8mb4_unicode_ci");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        }
    }
}
