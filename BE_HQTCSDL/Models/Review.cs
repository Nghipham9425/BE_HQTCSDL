using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("REVIEWS")]
    public class Review
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("USER_ID")]
        public long UserId { get; set; }

        [Column("PRODUCT_ID")]
        public long ProductId { get; set; }

        [Column("ORDER_ID")]
        public long OrderId { get; set; }

        [Column("RATING")]
        public int Rating { get; set; }
        // 1 - 5

        [Column("REVIEW_COMMENT")]
        public string? Comment { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;
    }
}
