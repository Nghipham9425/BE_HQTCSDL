using System.Text.Json;
using System.Threading.Tasks;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE_HQTCSDL.Controllers
{
    [ApiController]
    [Route("api/v1/payments")]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public PaymentWebhookController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("sepay/webhook")]
        public async Task<IActionResult> ReceiveSePayWebhook([FromBody] JsonElement payload)
        {
            var processed = await _orderService.ProcessSePayWebhookAsync(payload);

            // Return 200 to avoid repeated retries for unsupported payloads.
            return Ok(new
            {
                success = true,
                processed
            });
        }
    }
}