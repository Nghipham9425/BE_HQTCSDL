using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_HQTCSDL.Controllers
{
    [ApiController]
    [Route("api/v1/wishlist")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _service;

        public WishlistController(IWishlistService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

            var items = await _service.GetItemsAsync(userId);
            return Ok(new { items });
        }

        [HttpPost("{productId:long}")]
        public async Task<IActionResult> Add(long productId)
        {
            try
            {
                var userId = GetUserId();
                if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

                await _service.AddAsync(userId, productId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{productId:long}")]
        public async Task<IActionResult> Remove(long productId)
        {
            try
            {
                var userId = GetUserId();
                if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

                var removed = await _service.RemoveAsync(userId, productId);
                if (!removed) return NotFound(new { message = "Wishlist item not found" });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private long GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(id, out var userId) ? userId : 0;
        }
    }
}
