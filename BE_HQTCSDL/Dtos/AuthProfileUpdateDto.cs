namespace BE_HQTCSDL.Dtos
{
    public class AuthProfileUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Country { get; set; }
        public string? DefaultShippingAddress { get; set; }
    }
}
