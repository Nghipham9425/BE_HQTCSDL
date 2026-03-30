using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("USER_ADDRESSES")]
    public class UserAddress
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Required]
        [Column("USER_ID")]
        public long UserId { get; set; }

        [Column("ADDRESS_NAME")]
        [MaxLength(100)]
        public string? AddressName { get; set; }

        [Column("RECIPIENT_NAME")]
        [MaxLength(150)]
        public string? RecipientName { get; set; }

        [Required]
        [Column("FULL_ADDRESS")]
        [MaxLength(500)]
        public string FullAddress { get; set; } = string.Empty;

        [Column("CITY")]
        [MaxLength(100)]
        public string? City { get; set; }

        [Column("DISTRICT")]
        [MaxLength(100)]
        public string? District { get; set; }

        [Column("WARD")]
        [MaxLength(100)]
        public string? Ward { get; set; }

        [Column("POSTAL_CODE")]
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [Column("COUNTRY")]
        [MaxLength(100)]
        public string Country { get; set; } = "Vietnam";

        [Column("PHONE")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("IS_DEFAULT")]
        public int IsDefault { get; set; } = 0;

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
