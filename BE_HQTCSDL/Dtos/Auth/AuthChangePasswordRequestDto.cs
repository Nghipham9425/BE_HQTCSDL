using System.ComponentModel.DataAnnotations;

namespace BE_HQTCSDL.Dtos
{
    public class AuthChangePasswordRequestDto
    {
        [Required, MaxLength(255)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(255)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(255)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}