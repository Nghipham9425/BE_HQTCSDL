using System;

namespace BE_HQTCSDL.Dtos
{
    public class InventoryDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductSku { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class InventoryUpdateDto
    {
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
    }

    public class InventoryPagedResponseDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<InventoryDto> Items { get; set; } = new();
    }
}
