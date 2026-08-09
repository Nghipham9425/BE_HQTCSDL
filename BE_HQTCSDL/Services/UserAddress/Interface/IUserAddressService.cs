using System.Collections.Generic;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Services.Interfaces
{
    public interface IUserAddressService
    {
        Task<List<UserAddressDto>> GetByUserIdAsync(long userId);
        Task<UserAddressDto?> GetByIdAsync(long id, long userId);
        Task<UserAddressDto?> GetDefaultAsync(long userId);
        Task<UserAddressDto> CreateAsync(long userId, UserAddressUpsertDto dto);
        Task<UserAddressDto?> UpdateAsync(long id, long userId, UserAddressUpsertDto dto);
        Task<bool> DeleteAsync(long id, long userId);
        Task<bool> SetDefaultAsync(long id, long userId);
    }
}
