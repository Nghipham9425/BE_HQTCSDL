using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BE_HQTCSDL.Utils;

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

        [Column("COUNTRY")]
        [MaxLength(100)]
        public string? Country { get; set; }

        [Column("PHONE")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("ROLE")]
        [MaxLength(30)]
        public string Role { get; set; } = AppRoles.User;
        // USER | ADMIN | ORDER_MANAGER | INVENTORY_MANAGER

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        public ICollection<Order> Orders { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
        public ICollection<Wishlist> Wishlists { get; set; } = [];
        public ICollection<UserAddress> Addresses { get; set; } = [];
        public ICollection<Conversation> CustomerConversations { get; set; } = [];
        public ICollection<Conversation> AssignedConversations { get; set; } = [];
        public ICollection<ChatMessage> ChatMessages { get; set; } = [];
    }
}
