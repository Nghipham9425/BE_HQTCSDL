namespace BE_HQTCSDL.Dtos
{
    public class OrderLineItemDto
    {
        public long ProductId { get; set; }
        public string? Sku { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public long UnitPrice { get; set; }
        public int Quantity { get; set; }
        public long LineTotal { get; set; }
    }
}
