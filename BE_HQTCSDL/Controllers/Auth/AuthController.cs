using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Services.Interfaces;
using BE_HQTCSDL.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_HQTCSDL.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private const string RefreshCookieName = "refresh_token";
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRegisterRequestDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                AppendRefreshCookie(result.RefreshToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthLoginRequestDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                AppendRefreshCookie(result.RefreshToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var refreshToken = Request.Cookies[RefreshCookieName] ?? string.Empty;
                var result = await _authService.RefreshAsync(refreshToken);
                AppendRefreshCookie(result.RefreshToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                DeleteRefreshCookie();
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies[RefreshCookieName] ?? string.Empty;
            await _authService.LogoutAsync(refreshToken);
            DeleteRefreshCookie();
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _ = long.TryParse(idValue, out var userId);

            if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

            var profile = await _authService.GetProfileAsync(userId);
            if (profile == null) return NotFound(new { message = "User not found" });

            return Ok(profile);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] AuthProfileUpdateDto dto)
        {
            try
            {
                var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _ = long.TryParse(idValue, out var userId);

                if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

                var profile = await _authService.UpdateProfileAsync(userId, dto);
                if (profile == null) return NotFound(new { message = "User not found" });

                return Ok(profile);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] AuthChangePasswordRequestDto dto)
        {
            try
            {
                var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _ = long.TryParse(idValue, out var userId);

                if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

                await _authService.ChangePasswordAsync(userId, dto);
                DeleteRefreshCookie();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _authService.GetUsersAsync();
            return Ok(users);
        }

        [Authorize(Roles = AppRoles.Admin)]
        [HttpPut("users/{id:long}/role")]
        public async Task<IActionResult> UpdateUserRole(long id, [FromBody] AuthUpdateRoleRequestDto dto)
        {
            try
            {
                var updated = await _authService.UpdateUserRoleAsync(id, dto.Role);
                if (updated == null) return NotFound(new { message = "User not found" });

                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private void AppendRefreshCookie(string refreshToken)
        {
            // Cookie is scoped to auth endpoints and sent automatically by browser.
            Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/api/v1/auth",
                Expires = DateTimeOffset.UtcNow.AddDays(Config.Environment.RefreshTokenExpireDays)
            });
        }

        private void DeleteRefreshCookie()
        {
            Response.Cookies.Delete(RefreshCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/api/v1/auth"
            });
        }
    }
}
