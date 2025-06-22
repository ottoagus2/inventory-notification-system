using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }

        public DbSet<InventoryLog> InventoryLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InventoryLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProductData).IsRequired();
                entity.Property(e => e.ProcessedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.ProcessedBy).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            });
        }
    }
}