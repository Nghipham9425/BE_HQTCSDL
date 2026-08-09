using System.Collections.Generic;

namespace BE_HQTCSDL.Dtos
{
    public class ProductUpsertRequestDto
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProductType { get; set; } = "NORMAL";
        public long? Price { get; set; }
        public long? OriginalPrice { get; set; }
        public decimal? Weight { get; set; }
        public string? Descriptions { get; set; }
        public string? Thumbnail { get; set; }
        public string? Image { get; set; }
        public int Stock { get; set; } // Will be saved to Inventory table
        public bool IsActive { get; set; } = true;
        public string? CardId { get; set; }
        public List<long> CategoryIds { get; set; } = new();
    }
}