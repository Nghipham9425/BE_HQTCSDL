using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("ORDER_DETAILS")]
    public class OrderDetail
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("ORDER_ID")]
        public long OrderId { get; set; }

        [Column("PRODUCT_ID")]
        public long ProductId { get; set; }

        [Column("SKU")]
        [MaxLength(50)]
        public string? Sku { get; set; }

        [Column("PRICE")]
        public long Price { get; set; }

        [Column("QUANTITY")]
        public int Quantity { get; set; }

        // Navigation
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}
