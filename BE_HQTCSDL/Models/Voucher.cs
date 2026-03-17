using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("VOUCHERS")]
    public class Voucher
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Required]
        [Column("CODE")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

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

        [Column("MIN_ORDER_VALUE")]
        public long MinOrderValue { get; set; } = 0;

        [Column("MAX_DISCOUNT")]
        public long? MaxDiscount { get; set; }

        [Column("USAGE_LIMIT")]
        public int? UsageLimit { get; set; }

        [Required]
        [Column("START_DATE")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("END_DATE")]
        public DateTime EndDate { get; set; }

        [Column("IS_ACTIVE")]
        public int IsActive { get; set; } = 1;

        // Navigation
        public ICollection<Order> Orders { get; set; } = [];
    }
}
