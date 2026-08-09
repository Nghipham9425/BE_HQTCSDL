using System.Collections.Generic;

namespace BE_HQTCSDL.Dtos
{
    public class PaymentMethodPagedResponseDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<PaymentMethodListItemDto> Items { get; set; } = new();
    }
}
