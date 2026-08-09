namespace BE_HQTCSDL.Dtos
{
    public class VoucherPreviewResponseDto
    {
        public long OriginalAmount { get; set; }
        public long DiscountedAmount { get; set; }
        public long Discount { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
    }
}
