using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using System.Collections.Generic;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthTokenResponseDto> RegisterAsync(AuthRegisterRequestDto dto);
        Task<AuthTokenResponseDto> LoginAsync(AuthLoginRequestDto dto);
        Task<AuthTokenResponseDto> RefreshAsync(string refreshToken);
        Task ChangePasswordAsync(long userId, AuthChangePasswordRequestDto dto);
        Task<List<AuthUserDto>> GetUsersAsync();
        Task<AuthUserDto?> GetProfileAsync(long userId);
        Task<AuthUserDto?> UpdateProfileAsync(long userId, AuthProfileUpdateDto dto);
        Task<AuthUserDto?> UpdateUserRoleAsync(long userId, string role);

        Task LogoutAsync(string refreshToken);
    }
}