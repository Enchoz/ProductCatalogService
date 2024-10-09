using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Configration
{
    public class ProductDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventories { get; set; }

        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Inventories)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId);
        }
    }
}
