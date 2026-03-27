using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _db;

        public AuthRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> EmailExistsAsync(string normalizedEmail)
        {
            var count = await _db.Users.CountAsync(u => u.Email.ToLower() == normalizedEmail);
            return count > 0;
        }

        public Task<User?> GetUserByIdAsync(long userId)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public Task<User?> GetUserByEmailAsync(string normalizedEmail)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        }

        public Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token)
        {
            return _db.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();
            return refreshToken;
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = 1;
            await _db.SaveChangesAsync();
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
