using MedBridge.Models;
using Microsoft.EntityFrameworkCore;

namespace MedBridge.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<User> users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        //public DbSet<Product> Products { get; set; }
    }
}
