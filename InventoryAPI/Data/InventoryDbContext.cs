using Microsoft.EntityFrameworkCore;
using InventoryAPI.Models;

namespace InventoryAPI.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones específicas del modelo
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            });

            // Datos de ejemplo (seed data) - usando fechas fijas
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Laptop HP",
                    Description = "Laptop HP Pavilion 15.6 pulgadas",
                    Price = 899.99m,
                    Stock = 10,
                    Category = "Electronics",
                    CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = 2,
                    Name = "Mouse Inalámbrico",
                    Description = "Mouse inalámbrico ergonómico",
                    Price = 25.99m,
                    Stock = 50,
                    Category = "Accessories",
                    CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = 3,
                    Name = "Teclado Mecánico",
                    Description = "Teclado mecánico RGB retroiluminado",
                    Price = 79.99m,
                    Stock = 25,
                    Category = "Accessories",
                    CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}