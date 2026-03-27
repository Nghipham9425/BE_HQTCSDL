using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthTokenResponseDto> RegisterAsync(AuthRegisterRequestDto dto);
        Task<AuthTokenResponseDto> LoginAsync(AuthLoginRequestDto dto);
        Task<AuthTokenResponseDto> RefreshAsync(string refreshToken);
        Task<AuthUserDto?> GetProfileAsync(long userId);
        Task<AuthUserDto?> UpdateProfileAsync(long userId, AuthProfileUpdateDto dto);

        Task LogoutAsync(string refreshToken);
    }
}