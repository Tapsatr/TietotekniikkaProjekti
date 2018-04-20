using TietotekniikkaProjekti.Models;
using Microsoft.EntityFrameworkCore;


namespace TietotekniikkaProjekti.Data
{
    public class PassWordContext : DbContext
    {
        public PassWordContext(DbContextOptions<PassWordContext> options) : base(options)
        { }

        public DbSet<PasswordCode> PasswordCode { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PasswordCode>().ToTable("Secret");

        }
    }
}