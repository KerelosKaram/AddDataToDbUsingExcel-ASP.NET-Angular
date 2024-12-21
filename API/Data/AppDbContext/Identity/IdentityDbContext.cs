using API.Data.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace API.Data.AppDbContext.Identity
{
    public class IdentityDbContext : DbContext
    {
        public DbSet<AssignedRolesToEmployees>? AssignedRolesToEmployees { get; set; }
        public DbSet<Role>? Roles { get; set; }
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssignedRolesToEmployees>()
                .HasKey(are => are.RoleEmpID);  // Assuming RoleEmpID is the primary key
        }
    }
}