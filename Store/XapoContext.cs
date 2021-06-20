using Microsoft.EntityFrameworkCore;

namespace Core
{
    internal class XapoContext : DbContext
    {
        public XapoContext()
        {
        }

        public XapoContext(DbContextOptions options):base(options)
        {
            
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("DefaultConnection");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<XapoTransaction>().Property(x => x.Charge).HasPrecision(12, 8);
        }


        public DbSet<XapoTransaction> XapoTransaction { get; set; }
    }
}