using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        Task<InventoryPagedResponseDto> GetPagedAsync(string? q, int page, int pageSize);
        Task<InventoryDto?> GetByIdAsync(long id);
        Task<InventoryDto?> GetByProductIdAsync(long productId);
        Task<InventoryDto> CreateAsync(long productId, int quantity);
        Task<InventoryDto?> UpdateAsync(long id, InventoryUpdateDto dto);
        Task<bool> AdjustQuantityAsync(long productId, int adjustment);
        Task<bool> ReserveAsync(long productId, int quantity);
        Task<bool> ReleaseReservedAsync(long productId, int quantity);
        Task<bool> ConfirmReservedAsync(long productId, int quantity);
    }
}
