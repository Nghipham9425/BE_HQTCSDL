using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Repositories.Interfaces
{
    public interface IVoucherRepository
    {
        Task<VoucherPagedResponseDto> GetPagedAsync(string? q, bool? isActive, int page, int pageSize);
        Task<VoucherDetailDto?> GetByIdAsync(long id);
        Task<VoucherDetailDto> CreateAsync(VoucherUpsertDto dto);
        Task<VoucherDetailDto?> UpdateAsync(long id, VoucherUpsertDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
