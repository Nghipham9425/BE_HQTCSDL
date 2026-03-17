using System.Collections.Generic;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface ITcgCardService
    {
        Task<TcgCardPagedResponseDto> GetPagedAsync(string? setId, string? q, int page, int pageSize);
        Task<TcgCardDetailDto?> GetByIdAsync(string cardId);
        Task<List<TcgCardListItemDto>> GetBySetAsync(string setId);
        Task<TcgCardDetailDto> CreateAsync(TcgCardUpsertDto dto);
        Task<TcgCardDetailDto?> UpdateAsync(string cardId, TcgCardUpsertDto dto);
        Task<bool> DeleteAsync(string cardId);
    }
}