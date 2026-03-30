using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryPagedResponseDto> GetPagedAsync(string? q, int page, int pageSize);
        Task<InventoryDto?> GetByIdAsync(long id);
        Task<InventoryDto?> GetByProductIdAsync(long productId);
        Task<InventoryDto> CreateOrUpdateAsync(long productId, int quantity);
        Task<InventoryDto?> UpdateAsync(long id, InventoryUpdateDto dto);
        Task<bool> AdjustQuantityAsync(long productId, int adjustment);
    }
}
