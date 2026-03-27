using System.Collections.Generic;

namespace BE_HQTCSDL.Dtos
{
public class CategoryPagedResponseDto
{
public int Page { get; set; }
public int PageSize { get; set; }
public int Total { get; set; }
public List<CategoryListItemDto> Items { get; set; } = new();
}
}