using System;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_HQTCSDL.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IPaymentMethodService _service;

        public PaymentMethodController(IPaymentMethodService service)
        {
            _service = service;
        }

        [HttpGet("payment-methods")]
        public async Task<IActionResult> GetMethods(
            [FromQuery] string? q,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetPagedAsync(q, isActive, page, pageSize);
            return Ok(result);
        }

        [HttpGet("payment-methods/{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Payment method not found" });
            return Ok(result);
        }

        [HttpPost("payment-methods")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] PaymentMethodUpsertDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("payment-methods/{id:long}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(long id, [FromBody] PaymentMethodUpsertDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                if (result == null) return NotFound(new { message = "Payment method not found" });
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("payment-methods/{id:long}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(long id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Payment method not found or is in use" });
            return NoContent();
        }
    }
}
