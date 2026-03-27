public class CategoryDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Thumbnail { get; set; }

    public int ProductCount { get; set; }

    
}