using System.Threading.Tasks;
using BE_HQTCSDL.Models;

namespace BE_HQTCSDL.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> EmailExistsAsync(string normalizedEmail);
        Task<User?> GetUserByIdAsync(long userId);
        Task<User?> GetUserByEmailAsync(string normalizedEmail);
        Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<User> CreateUserAsync(User user);
        Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(RefreshToken refreshToken);
        Task SaveChangesAsync();
    }
}
