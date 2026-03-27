using System.ComponentModel.DataAnnotations;

namespace BE_HQTCSDL.Dtos
{
    public class AuthRegisterRequestDto
    {
        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;
    }
}