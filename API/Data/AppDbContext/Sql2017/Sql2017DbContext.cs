using API.Data.Models.Entities.Sql2017;
using Microsoft.EntityFrameworkCore;

namespace API.Data.AppDbContext.Sql2017DbContext

{
    public class Sql2017DbContext : DbContext
    {

        public DbSet<QSCustomerBrandTarget> QSCustomerBrandTarget { get; set; }
        public DbSet<QSCustomerTarget> QSCustomerTarget { get; set; }
        
        public Sql2017DbContext(DbContextOptions<Sql2017DbContext> options) : base(options) 
        { 

        }

    }
}