using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("CATEGORIES")]
    public class Category
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Required]
        [Column("NAME")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("DESCRIPTION")]
        public string? Description { get; set; }

        [Column("THUMBNAIL")]
        [MaxLength(500)]
        public string? Thumbnail { get; set; }

        // Navigation
        public ICollection<ProductCategory> ProductCategories { get; set; } = [];
    }
}
