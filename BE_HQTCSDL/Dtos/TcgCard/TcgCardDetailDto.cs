namespace BE_HQTCSDL.Dtos
{
    public class TcgCardDetailDto : TcgCardListItemDto
    {
        public string? Series { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}