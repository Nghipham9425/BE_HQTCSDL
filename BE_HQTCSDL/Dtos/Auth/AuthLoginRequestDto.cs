using System.ComponentModel.DataAnnotations;

namespace BE_HQTCSDL.Dtos
{
    public class AuthLoginRequestDto
    {
        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Password { get; set; } = string.Empty;
    }
}