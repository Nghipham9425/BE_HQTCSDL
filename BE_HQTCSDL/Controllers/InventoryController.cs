using System;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_HQTCSDL.Controllers
{
    [ApiController]
    [Route("api/v1/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryController(IInventoryService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetPagedAsync(q, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetById(long id)
        {
            var inventory = await _service.GetByIdAsync(id);
            if (inventory == null) return NotFound(new { message = "Inventory not found" });

            return Ok(inventory);
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(long productId)
        {
            var inventory = await _service.GetByProductIdAsync(productId);
            if (inventory == null) return NotFound(new { message = "Inventory not found for this product" });

            return Ok(inventory);
        }

        [HttpPost("product/{productId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateOrUpdate(long productId, [FromBody] InventoryUpdateDto dto)
        {
            try
            {
                var inventory = await _service.CreateOrUpdateAsync(productId, dto.Quantity);
                return Ok(inventory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(long id, [FromBody] InventoryUpdateDto dto)
        {
            try
            {
                var inventory = await _service.UpdateAsync(id, dto);
                if (inventory == null) return NotFound(new { message = "Inventory not found" });

                return Ok(inventory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("product/{productId}/adjust")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Adjust(long productId, [FromBody] InventoryAdjustDto dto)
        {
            var result = await _service.AdjustQuantityAsync(productId, dto.Adjustment);
            if (!result) return NotFound(new { message = "Inventory not found for this product" });

            return Ok(new { message = "Quantity adjusted successfully" });
        }
    }

    public class InventoryAdjustDto
    {
        public int Adjustment { get; set; }
    }
}
