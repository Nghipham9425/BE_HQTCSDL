using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("WISHLISTS")]
    public class Wishlist
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("USER_ID")]
        public long UserId { get; set; }

        [Column("PRODUCT_ID")]
        public long ProductId { get; set; }

        [Column("ADDED_AT")]
        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}
