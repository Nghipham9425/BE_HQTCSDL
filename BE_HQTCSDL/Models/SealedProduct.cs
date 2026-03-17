using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("SEALED_PRODUCTS")]
    public class SealedProduct
    {
        [Key]
        [Column("PRODUCT_ID")]
        public long ProductId { get; set; }

        [Column("SET_ID")]
        [MaxLength(20)]
        public string? SetId { get; set; }

        [Column("PACK_TYPE")]
        [MaxLength(30)]
        public string? PackType { get; set; }
        // BOOSTER_BOX | BOOSTER_PACK | ETB | BUNDLE

        // Navigation
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [ForeignKey("SetId")]
        public TcgSet? Set { get; set; }
    }
}
