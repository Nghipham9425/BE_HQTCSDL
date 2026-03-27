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
    public class TcgSetController : ControllerBase
    {
        private readonly ITcgSetService _service;

        public TcgSetController(ITcgSetService service)
        {
            _service = service;
        }

        [HttpGet("tcg-sets")]
        public async Task<IActionResult> GetSets([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetPagedAsync(q, page, pageSize);
            return Ok(result);
        }

        [HttpGet("tcg-sets/{setId}")]
        public async Task<IActionResult> GetById(string setId)
        {
            try
            {
                var result = await _service.GetByIdAsync(setId);
                if (result == null) return NotFound(new { message = "Set not found" });
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("tcg-sets")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] TcgSetUpsertDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { setId = result.SetId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("tcg-sets/{setId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(string setId, [FromBody] TcgSetUpsertDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(setId, dto);
                if (result == null) return NotFound(new { message = "Set not found" });
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("tcg-sets/{setId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(string setId)
        {
            try
            {
                var success = await _service.DeleteAsync(setId);
                if (!success) return NotFound(new { message = "Set not found or has related cards/products" });
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
