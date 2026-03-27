using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_HQTCSDL.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderPlaceRequestDto dto)
        {
            try
            {
                var customerId = GetCurrentUserId();
                if (customerId <= 0) return Unauthorized(new { message = "Unauthorized" });

                var result = await _service.PlaceOrderAsync(customerId, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var customerId = GetCurrentUserId();
                if (customerId <= 0) return Unauthorized(new { message = "Unauthorized" });

                var result = await _service.GetMyOrdersAsync(customerId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("me/{id:long}")]
        public async Task<IActionResult> GetMyOrderById(long id)
        {
            try
            {
                var customerId = GetCurrentUserId();
                if (customerId <= 0) return Unauthorized(new { message = "Unauthorized" });

                var result = await _service.GetMyOrderByIdAsync(customerId, id);
                if (result == null) return NotFound(new { message = "Order not found" });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminOrders(
            [FromQuery] string? q,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _service.GetAdminOrdersAsync(q, status, page, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin/{id:long}")]
        public async Task<IActionResult> GetAdminOrderById(long id)
        {
            try
            {
                var result = await _service.GetAdminOrderByIdAsync(id);
                if (result == null) return NotFound(new { message = "Order not found" });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("admin/{id:long}/status")]
        public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] OrderUpdateStatusDto dto)
        {
            try
            {
                var updated = await _service.UpdateOrderStatusAsync(id, dto.Status);
                if (!updated) return NotFound(new { message = "Order not found" });

                var result = await _service.GetAdminOrderByIdAsync(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private long GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(id, out var customerId) ? customerId : 0;
        }
    }
}
