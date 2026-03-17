using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("TCG_SET")]
    public class TcgSet
    {
        [Key]
        [Column("SET_ID")]
        [MaxLength(20)]
        public string SetId { get; set; } = string.Empty;

        [Required]
        [Column("NAME")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("SERIES")]
        [MaxLength(100)]
        public string? Series { get; set; }

        [Column("RELEASE_DATE")]
        public DateTime? ReleaseDate { get; set; }

        [Column("TOTAL_CARDS")]
        public int? TotalCards { get; set; }

        [Column("LOGO_URL")]
        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        // Navigation
        public ICollection<TcgCard> Cards { get; set; } = [];
        public ICollection<SealedProduct> SealedProducts { get; set; } = [];
    }
}
