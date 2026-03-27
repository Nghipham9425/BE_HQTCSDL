using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface IPaymentMethodService
    {
        Task<PaymentMethodPagedResponseDto> GetPagedAsync(string? q, bool? isActive, int page, int pageSize);
        Task<PaymentMethodDetailDto?> GetByIdAsync(long id);
        Task<PaymentMethodDetailDto> CreateAsync(PaymentMethodUpsertDto dto);
        Task<PaymentMethodDetailDto?> UpdateAsync(long id, PaymentMethodUpsertDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
