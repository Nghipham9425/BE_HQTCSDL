namespace BE_HQTCSDL.Dtos
{
    public class AuthTokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;

        public DateTime AccessTokenExpiresAt { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string RefreshToken { get; set; } = string.Empty;

        public AuthUserDto User { get; set; } = new();
    }
}