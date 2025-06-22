using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class InventoryLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE

        [Required]
        public string ProductData { get; set; } = string.Empty; // JSON del producto

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? ProcessedBy { get; set; }

        public bool IsSuccessful { get; set; } = true;

        [StringLength(500)]
        public string? ErrorMessage { get; set; }
    }
}