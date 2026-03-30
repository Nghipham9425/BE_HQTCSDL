using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace BE_HQTCSDL.Services
{
	public class AuthService : IAuthService
	{
		private readonly IAuthRepository _repo;

		public AuthService(IAuthRepository repo)
		{
			_repo = repo;
		}

		public async Task<AuthTokenResponseDto> RegisterAsync(AuthRegisterRequestDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Email is required");
			if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password is required");
			if (string.IsNullOrWhiteSpace(dto.FullName)) throw new ArgumentException("FullName is required");

			var email = dto.Email.Trim().ToLowerInvariant();
			var fullName = dto.FullName.Trim();

			var exists = await _repo.EmailExistsAsync(email);
			if (exists) throw new ArgumentException("Email already exists");

			var user = new User
			{
				Email = email,
				Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
				FullName = fullName,
				Role = "USER",
				CreatedAt = DateTime.UtcNow
			};

			await _repo.CreateUserAsync(user);

			return await IssueSessionAsync(user);
		}

		public async Task<AuthTokenResponseDto> LoginAsync(AuthLoginRequestDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Email is required");
			if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password is required");

			var email = dto.Email.Trim().ToLowerInvariant();
			var user = await _repo.GetUserByEmailAsync(email);
			if (user == null) throw new UnauthorizedAccessException("Invalid credentials");

			var passwordValid = VerifyPassword(dto.Password, user.Password);
			if (!passwordValid) throw new UnauthorizedAccessException("Invalid credentials");

			if (!LooksLikeBcryptHash(user.Password))
			{
				user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
				await _repo.SaveChangesAsync();
			}

			return await IssueSessionAsync(user);
		}

		public async Task<AuthTokenResponseDto> RefreshAsync(string refreshToken)
		{
			if (string.IsNullOrWhiteSpace(refreshToken))
			{
				throw new UnauthorizedAccessException("Refresh token is required");
			}

			var tokenRow = await _repo.GetRefreshTokenWithUserAsync(refreshToken);

			if (tokenRow == null || tokenRow.IsRevoked == 1 || tokenRow.ExpiresAt <= DateTime.UtcNow)
			{
				throw new UnauthorizedAccessException("Invalid or expired refresh token");
			}

			await _repo.RevokeRefreshTokenAsync(tokenRow);

			var newTokenValue = GenerateRefreshToken();
			var newToken = new RefreshToken
			{
				UserId = tokenRow.UserId,
				Token = newTokenValue,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddDays(BE_HQTCSDL.Config.Environment.RefreshTokenExpireDays),
				IsRevoked = 0
			};

			await _repo.CreateRefreshTokenAsync(newToken);

			return BuildTokenResponse(tokenRow.User, newTokenValue);
		}

		public async Task LogoutAsync(string refreshToken)
		{
			if (string.IsNullOrWhiteSpace(refreshToken)) return;

			var tokenRow = await _repo.GetRefreshTokenAsync(refreshToken);
			if (tokenRow == null) return;

			await _repo.RevokeRefreshTokenAsync(tokenRow);
		}

		public async Task<AuthUserDto?> GetProfileAsync(long userId)
		{
			if (userId <= 0) throw new ArgumentException("Invalid user id");

			var user = await _repo.GetUserByIdAsync(userId);
			if (user == null) return null;

			return MapAuthUser(user);
		}

		public async Task<AuthUserDto?> UpdateProfileAsync(long userId, AuthProfileUpdateDto dto)
		{
			if (userId <= 0) throw new ArgumentException("Invalid user id");
			if (dto == null) throw new ArgumentException("Payload is required");
			if (string.IsNullOrWhiteSpace(dto.FullName)) throw new ArgumentException("FullName is required");

			var user = await _repo.GetUserByIdAsync(userId);
			if (user == null) return null;

			var fullName = dto.FullName.Trim();
			if (fullName.Length > 150) throw new ArgumentException("FullName too long");

			var phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
			if (phone != null && phone.Length > 20) throw new ArgumentException("Phone too long");

			var country = string.IsNullOrWhiteSpace(dto.Country) ? null : dto.Country.Trim();
			if (country != null && country.Length > 100) throw new ArgumentException("Country too long");

			user.FullName = fullName;
			user.Phone = phone;
			user.Country = country;

			await _repo.SaveChangesAsync();

			return MapAuthUser(user);
		}

		private async Task<AuthTokenResponseDto> IssueSessionAsync(User user)
		{
			var refreshTokenValue = GenerateRefreshToken();

			var refreshToken = new RefreshToken
			{
				UserId = user.Id,
				Token = refreshTokenValue,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddDays(BE_HQTCSDL.Config.Environment.RefreshTokenExpireDays),
				IsRevoked = 0
			};

			await _repo.CreateRefreshTokenAsync(refreshToken);

			return BuildTokenResponse(user, refreshTokenValue);
		}

		private AuthTokenResponseDto BuildTokenResponse(User user, string refreshToken)
		{
			var now = DateTime.UtcNow;
			var expiresAt = now.AddHours(BE_HQTCSDL.Config.Environment.JwtExpireHours);

			var claims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new(ClaimTypes.Email, user.Email),
				new(ClaimTypes.Name, user.FullName),
				new(ClaimTypes.Role, user.Role)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(BE_HQTCSDL.Config.Environment.JwtSecret));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var jwt = new JwtSecurityToken(
				issuer: BE_HQTCSDL.Config.Environment.JwtIssuer,
				audience: BE_HQTCSDL.Config.Environment.JwtAudience,
				claims: claims,
				notBefore: now,
				expires: expiresAt,
				signingCredentials: credentials);

			var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

			return new AuthTokenResponseDto
			{
				AccessToken = accessToken,
				AccessTokenExpiresAt = expiresAt,
				RefreshToken = refreshToken,
				User = MapAuthUser(user)
			};
		}

		private static AuthUserDto MapAuthUser(User user)
		{
			return new AuthUserDto
			{
				Id = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				Phone = user.Phone,
				Country = user.Country,
				Role = user.Role
			};
		}

		private static string GenerateRefreshToken()
		{
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
		}

		private static bool VerifyPassword(string plainPassword, string storedPassword)
		{
			if (LooksLikeBcryptHash(storedPassword))
			{
				return BCrypt.Net.BCrypt.Verify(plainPassword, storedPassword);
			}

			return plainPassword == storedPassword;
		}

		private static bool LooksLikeBcryptHash(string value)
		{
			return value.StartsWith("$2a$") || value.StartsWith("$2b$") || value.StartsWith("$2y$");
		}
	}
}
