using API.Data.Models.Entities.DBAX;
using Microsoft.EntityFrameworkCore;

namespace API.Data.AppDbContext.DBAX
{
    public class DBAXDbContext : DbContext
    {
        public DbSet<ElamirCustomerClassification2> ElamirCustomerClassification2 { get; set; }
        public DBAXDbContext(DbContextOptions<DBAXDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ElamirCustomerClassification2>(entity =>
            {
                entity.ToTable("ElamirCustomerClassification2").HasKey(x => x.ElamirCustomerClassificationID);
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}