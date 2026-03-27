using System.Collections.Generic;

namespace BE_HQTCSDL.Dtos
{
    public class OrderPagedResponseDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<OrderListItemDto> Items { get; set; } = new();
    }
}
