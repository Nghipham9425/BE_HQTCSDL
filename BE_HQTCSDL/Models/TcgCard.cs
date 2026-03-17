using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("TCG_CARD")]
    public class TcgCard
    {
        [Key]
        [Column("CARD_ID")]
        [MaxLength(30)]
        public string CardId { get; set; } = string.Empty;

        [Required]
        [Column("SET_ID")]
        [MaxLength(20)]
        public string SetId { get; set; } = string.Empty;

        [Required]
        [Column("NAME")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("CARD_NUMBER")]
        [MaxLength(10)]
        public string CardNumber { get; set; } = string.Empty;

        [Column("RARITY")]
        [MaxLength(80)]
        public string? Rarity { get; set; }

        [Column("IMAGE_SMALL")]
        [MaxLength(500)]
        public string? ImageSmall { get; set; }

        [Column("IMAGE_LARGE")]
        [MaxLength(500)]
        public string? ImageLarge { get; set; }

        [Column("IS_DELETED")]
        public bool IsDeleted { get; set; } = false;

        // Navigation
        [ForeignKey("SetId")]
        public TcgSet Set { get; set; } = null!;

        public ICollection<Product> Products { get; set; } = [];
    }
}
