namespace BE_HQTCSDL.Dtos
{
    public class TcgSetListItemDto
    {
        public string SetId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Series { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? TotalCards { get; set; }
        public string? LogoUrl { get; set; }
        public int CardCount { get; set; }
    }
}
