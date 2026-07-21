namespace BE_HQTCSDL.Utils;

public static class Pagination
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        return (
            Math.Max(page, 1),
            pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize));
    }
}
