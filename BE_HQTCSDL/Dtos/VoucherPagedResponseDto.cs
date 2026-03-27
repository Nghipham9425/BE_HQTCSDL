using System.Collections.Generic;

namespace BE_HQTCSDL.Dtos
{
    public class VoucherPagedResponseDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<VoucherListItemDto> Items { get; set; } = new();
    }
}
