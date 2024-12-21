using API.Data.Models.Entities.OneNineTwo;
using Microsoft.EntityFrameworkCore;

namespace API.Data.AppDbContext.OneNineTwo
{
    public class OneNineTwoDbContext : DbContext
    {
        public DbSet<MessageOut>? MessageOut { get; set; }
        public OneNineTwoDbContext(DbContextOptions<OneNineTwoDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageOut>(entity =>
            {
                entity.ToTable("MessageOut"); // Specify table name if different

                entity.HasKey(e => e.Id);

                entity.Property(e => e.MessageTo)
                    .HasMaxLength(80);

                entity.Property(e => e.MessageFrom)
                    .HasMaxLength(80);

                entity.Property(e => e.MessageType)
                    .HasMaxLength(80);

                entity.Property(e => e.Gateway)
                    .HasMaxLength(80);

                entity.Property(e => e.UserId)
                    .HasMaxLength(80);

                entity.Property(e => e.MessageText)
                    .HasColumnType("nvarchar(MAX)");

                entity.Property(e => e.UserInfo)
                    .HasColumnType("nvarchar(MAX)");

                entity.Property(e => e.Priority)
                    .IsRequired(false);

                entity.Property(e => e.Scheduled)
                    .IsRequired(false);

                entity.Property(e => e.ValidityPeriod)
                    .IsRequired(false);

                entity.Property(e => e.IsRead)
                    .IsRequired();

                entity.Property(e => e.IsSent)
                    .IsRequired();
            });
            
            base.OnModelCreating(modelBuilder);
        }
    }
}