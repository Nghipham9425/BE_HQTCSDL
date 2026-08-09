using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("REFRESH_TOKENS")]
    public class RefreshToken
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("USER_ID")]
        public long UserId { get; set; }

        [Required]
        [Column("TOKEN")]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        [Column("EXPIRES_AT")]
        public DateTime ExpiresAt { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("IS_REVOKED")]
        public int IsRevoked { get; set; } = 0;

        // Navigation
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
