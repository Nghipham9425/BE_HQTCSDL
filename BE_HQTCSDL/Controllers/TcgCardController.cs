using BE_HQTCSDL.Services.Interfaces;
using BE_HQTCSDL.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BE_HQTCSDL.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class TcgCardController : ControllerBase
    {
        private readonly ITcgCardService _service;

        public TcgCardController(ITcgCardService service)
        {
            _service = service;
        }

        [HttpGet("tcg-cards")]
        public async Task<IActionResult> GetCards([FromQuery] string? setId, [FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetPagedAsync(setId, q, page, pageSize);
            return Ok(result);
        }

        [HttpGet("tcg-cards/{cardId}")]
        public async Task<IActionResult> GetById(string cardId)
        {
            var result = await _service.GetByIdAsync(cardId);
            if (result is null) return NotFound(new { message = "Card not found" });
            return Ok(result);
        }

        [HttpGet("tcg-sets/{setId}/cards")]
        public async Task<IActionResult> GetBySet(string setId)
        {
            var result = await _service.GetBySetAsync(setId);
            return Ok(result);
        }

        [HttpPost("tcg-cards")]
        public async Task<IActionResult> Create([FromBody] TcgCardUpsertDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { cardId = result.CardId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("tcg-cards/{cardId}")]
        public async Task<IActionResult> Update(string cardId, [FromBody] TcgCardUpsertDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(cardId, dto);
                if (result is null) return NotFound(new { message = "Card not found" });
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("tcg-cards/{cardId}")]
        public async Task<IActionResult> Delete(string cardId)
        {
            var success = await _service.DeleteAsync(cardId);
            if (!success) return NotFound(new { message = "Card not found" });
            return NoContent();
        }
    }
}