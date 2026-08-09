using System;

namespace BE_HQTCSDL.Dtos
{
    public class VoucherDetailDto
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public long DiscountValue { get; set; }
        public long MinOrderValue { get; set; }
        public long? MaxDiscount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int UsedCount { get; set; }
    }
}
