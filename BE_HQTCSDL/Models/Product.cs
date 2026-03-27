using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("PRODUCTS")]
    public class Product
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Required]
        [Column("SKU")]
        [MaxLength(50)]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [Column("NAME")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column("PRODUCT_TYPE")]
        [MaxLength(20)]
        public string ProductType { get; set; } = "NORMAL";
        // NORMAL | TCG_CARD | CONSOLE | ACCESSORY

        [Column("PRICE")]
        public long? Price { get; set; }

        [Column("ORIGINAL_PRICE")]
        public long? OriginalPrice { get; set; }

        [Column("WEIGHT")]
        public decimal? Weight { get; set; }

        [Column("DESCRIPTIONS")]
        public string? Descriptions { get; set; }

        [Column("THUMBNAIL")]
        [MaxLength(500)]
        public string? Thumbnail { get; set; }

        [Column("IMAGE")]
        [MaxLength(500)]
        public string? Image { get; set; }

        [Column("CREATE_DATE")]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column("STOCK")]
        public int Stock { get; set; } = 0;

        [Column("IS_ACTIVE")]
        public int IsActive { get; set; } = 1;

        [Column("CARD_ID")]
        [MaxLength(30)]
        public string? CardId { get; set; }

        // Navigation
        [ForeignKey("CardId")]
        public TcgCard? Card { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; } = [];
        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
        public ICollection<Wishlist> Wishlists { get; set; } = [];
    }
}
