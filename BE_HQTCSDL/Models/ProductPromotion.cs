using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("PRODUCT_PROMOTIONS")]
    public class ProductPromotion
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("PRODUCT_ID")]
        public long ProductId { get; set; }

        [Column("PROMOTION_ID")]
        public long PromotionId { get; set; }

        // Navigation
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [ForeignKey("PromotionId")]
        public Promotion Promotion { get; set; } = null!;
    }
}
