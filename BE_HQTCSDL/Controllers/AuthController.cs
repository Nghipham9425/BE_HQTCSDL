using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Services.Interfaces;
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

        private void AppendRefreshCookie(string refreshToken)
        {
            // Cookie is scoped to auth endpoints and sent automatically by browser.
            Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/api/v1/auth",
                Expires = DateTimeOffset.UtcNow.AddDays(BE_HQTCSDL.Config.Environment.RefreshTokenExpireDays)
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
