namespace BE_HQTCSDL.Dtos
{
    public class AuthUserDto
    {
        public long Id { get; set; }
        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Country { get; set; }
        public string? DefaultShippingAddress { get; set; }

        public string Role { get; set; } = "USER";
    }
}