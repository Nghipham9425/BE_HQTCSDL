using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("PRODUCT_CATEGORIES")]
    public class ProductCategory
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("PRODUCT_ID")]
        public long ProductId { get; set; }

        [Column("CATEGORY_ID")]
        public long CategoryId { get; set; }

        // Navigation
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;
    }
}
