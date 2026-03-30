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
    [Route("api/v1/addresses")]
    [Authorize]
    public class UserAddressController : ControllerBase
    {
        private readonly IUserAddressService _service;

        public UserAddressController(IUserAddressService service)
        {
            _service = service;
        }

        private long GetUserId()
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _ = long.TryParse(idValue, out var userId);
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

            var addresses = await _service.GetByUserIdAsync(userId);
            return Ok(new { items = addresses });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

            var address = await _service.GetByIdAsync(id, userId);
            if (address == null) return NotFound(new { message = "Address not found" });

            return Ok(address);
        }

        [HttpGet("default")]
        public async Task<IActionResult> GetDefault()
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

            var address = await _service.GetDefaultAsync(userId);
            if (address == null) return NotFound(new { message = "No default address found" });

            return Ok(address);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserAddressUpsertDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

                var address = await _service.CreateAsync(userId, dto);
                return CreatedAtAction(nameof(GetById), new { id = address.Id }, address);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UserAddressUpsertDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

                var address = await _service.UpdateAsync(id, userId, dto);
                if (address == null) return NotFound(new { message = "Address not found" });

                return Ok(address);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

            var result = await _service.DeleteAsync(id, userId);
            if (!result) return NotFound(new { message = "Address not found" });

            return NoContent();
        }

        [HttpPost("{id}/set-default")]
        public async Task<IActionResult> SetDefault(long id)
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized(new { message = "Unauthorized" });

            var result = await _service.SetDefaultAsync(id, userId);
            if (!result) return NotFound(new { message = "Address not found" });

            return Ok(new { message = "Default address updated" });
        }
    }
}
