using API.Data.Models.Entities.DbElWagd;
using Microsoft.EntityFrameworkCore;

namespace API.Data.AppDbContext.DbElWagd
{
    public class DbElWagdDbContext : DbContext
    {
        public DbSet<PSKUItemElamir>? PSKUItemElamir { get; set; }
        
        public DbElWagdDbContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PSKUItemElamir>(entity =>
            {
                entity.ToTable("PSKUItemElamir").HasKey(x => x.PSKUItemID);
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}