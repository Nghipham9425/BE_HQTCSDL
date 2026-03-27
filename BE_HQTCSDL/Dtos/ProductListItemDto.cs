using System;

namespace BE_HQTCSDL.Dtos
{
    public class ProductListItemDto
    {
        public long Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProductType { get; set; } = "NORMAL";
        public long? Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string? Thumbnail { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
