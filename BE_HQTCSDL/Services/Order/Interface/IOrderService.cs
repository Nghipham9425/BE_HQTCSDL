using System.Collections.Generic;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using System.Text.Json;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDetailDto> PlaceOrderAsync(long customerId, OrderPlaceRequestDto dto);
        Task<List<OrderListItemDto>> GetMyOrdersAsync(long customerId);
        Task<OrderDetailDto?> GetMyOrderByIdAsync(long customerId, long orderId);
        Task<OrderDetailDto?> CancelMyOrderAsync(long customerId, long orderId);
        Task<OrderPagedResponseDto> GetAdminOrdersAsync(string? q, string? status, int page, int pageSize);
        Task<OrderDetailDto?> GetAdminOrderByIdAsync(long orderId);
        Task<bool> UpdateOrderStatusAsync(long orderId, string status);
        Task<bool> ProcessSePayWebhookAsync(JsonElement payload);
    }
}