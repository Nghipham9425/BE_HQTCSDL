using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface ITcgSetService
    {
        Task<TcgSetPagedResponseDto> GetPagedAsync(string? q, int page, int pageSize);
        Task<TcgSetDetailDto?> GetByIdAsync(string setId);
        Task<TcgSetDetailDto> CreateAsync(TcgSetUpsertDto dto);
        Task<TcgSetDetailDto?> UpdateAsync(string setId, TcgSetUpsertDto dto);
        Task<bool> DeleteAsync(string setId);
    }
}
