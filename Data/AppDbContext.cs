using Back_for_hackathon.Models;
using Microsoft.EntityFrameworkCore;

namespace Back_for_hackathon.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .Property(x => x.Subtotal)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Order>()
                .Property(x => x.CompositeTaxRate)
                .HasColumnType("numeric(10,6)");

            modelBuilder.Entity<Order>()
                .Property(x => x.TaxAmount)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Order>()
                .Property(x => x.TotalAmount)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Order>()
                .Property(x => x.StateRate)
                .HasColumnType("numeric(10,6)");

            modelBuilder.Entity<Order>()
                .Property(x => x.CountyRate)
                .HasColumnType("numeric(10,6)");

            modelBuilder.Entity<Order>()
                .Property(x => x.CityRate)
                .HasColumnType("numeric(10,6)");
        }
    }
}
