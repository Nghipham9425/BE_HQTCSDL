using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryPagedResponseDto> GetPagedAsync(string? q, int page, int pageSize);
        Task<CategoryDetailDto?> GetByIdAsync(long id);
        Task<CategoryDetailDto> CreateAsync(CategoryUpsertRequestDto dto);
        Task<CategoryDetailDto?> UpdateAsync(long id, CategoryUpsertRequestDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
