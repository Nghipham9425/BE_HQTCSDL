using System.Collections.Generic;

namespace BE_HQTCSDL.Dtos
{
    public class TcgSetPagedResponseDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<TcgSetListItemDto> Items { get; set; } = new();
    }
}
