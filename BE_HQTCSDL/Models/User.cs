using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("USERS")]
    public class User
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Required]
        [Column("EMAIL")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("PASSWORD")]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Column("FULL_NAME")]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Column("DEFAULT_SHIPPING_ADDRESS")]
        [MaxLength(300)]
        public string? DefaultShippingAddress { get; set; }

        [Column("COUNTRY")]
        [MaxLength(100)]
        public string? Country { get; set; }

        [Column("PHONE")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("ROLE")]
        [MaxLength(10)]
        public string Role { get; set; } = "USER";
        // USER | ADMIN

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        public ICollection<Order> Orders { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
        public ICollection<Wishlist> Wishlists { get; set; } = [];
    }
}
