namespace BE_HQTCSDL.Dtos
{
    public class RevenueStatsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalItems { get; set; }
        public decimal AvgOrderValue { get; set; }
    }

    public class InventoryStatsItemDto
    {
        public long ProductId { get; set; }
        public string? Sku { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public bool IsLowStock { get; set; }
    }

    public class InventoryStatsResponseDto
    {
        public int LowStockThreshold { get; set; }
        public List<InventoryStatsItemDto> Items { get; set; } = new();
    }
}
