namespace BE_HQTCSDL.Dtos
{
    public class TcgCardListItemDto
    {
        public string CardId { get; set; } = string.Empty;
        public string SetId { get; set; } = string.Empty;
        public string? SetName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string? Rarity { get; set; }
        public string? ImageSmall { get; set; }
        public string? ImageLarge { get; set; }
    }
}