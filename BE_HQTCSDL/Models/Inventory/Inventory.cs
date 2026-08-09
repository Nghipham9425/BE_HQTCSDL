using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("INVENTORY")]
    public class Inventory
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Required]
        [Column("PRODUCT_ID")]
        public long ProductId { get; set; }

        [Column("QUANTITY")]
        public int Quantity { get; set; } = 0;

        [Column("RESERVED_QUANTITY")]
        public int ReservedQuantity { get; set; } = 0;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // Computed property for available stock
        [NotMapped]
        public int AvailableQuantity => Quantity - ReservedQuantity;
    }
}
