using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Repositories.Interfaces
{
    public interface IPaymentMethodRepository
    {
        Task<PaymentMethodPagedResponseDto> GetPagedAsync(string? q, bool? isActive, int page, int pageSize);
        Task<PaymentMethodDetailDto?> GetByIdAsync(long id);
        Task<PaymentMethodDetailDto> CreateAsync(PaymentMethodUpsertDto dto);
        Task<PaymentMethodDetailDto?> UpdateAsync(long id, PaymentMethodUpsertDto dto);
        Task<bool> DeleteAsync(long id);
    }
}
