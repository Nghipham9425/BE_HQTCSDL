using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("PROMOTIONS")]
    public class Promotion
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Required]
        [Column("NAME")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("DISCOUNT_TYPE")]
        [MaxLength(10)]
        public string DiscountType { get; set; } = string.Empty;
        // PERCENT | FIXED

        [Column("DISCOUNT_VALUE")]
        public long DiscountValue { get; set; }

        [Column("START_DATE")]
        public DateTime StartDate { get; set; }

        [Column("END_DATE")]
        public DateTime EndDate { get; set; }

        [Column("IS_ACTIVE")]
        public int IsActive { get; set; } = 1;

        // Navigation
        public ICollection<ProductPromotion> ProductPromotions { get; set; } = [];
    }
}
