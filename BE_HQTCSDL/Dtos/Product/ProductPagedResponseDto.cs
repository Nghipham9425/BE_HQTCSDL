using System.Collections.Generic;

namespace BE_HQTCSDL.Dtos
{
    public class ProductPagedResponseDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<ProductListItemDto> Items { get; set; } = new();
    }
}