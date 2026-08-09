using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;

namespace BE_HQTCSDL.Services
{
    public class UserAddressService : IUserAddressService
    {
        private readonly IUserAddressRepository _repo;

        public UserAddressService(IUserAddressRepository repo)
        {
            _repo = repo;
        }

        public Task<List<UserAddressDto>> GetByUserIdAsync(long userId)
        {
            return _repo.GetByUserIdAsync(userId);
        }

        public Task<UserAddressDto?> GetByIdAsync(long id, long userId)
        {
            return _repo.GetByIdAsync(id, userId);
        }

        public Task<UserAddressDto?> GetDefaultAsync(long userId)
        {
            return _repo.GetDefaultAsync(userId);
        }

        public Task<UserAddressDto> CreateAsync(long userId, UserAddressUpsertDto dto)
        {
            ValidateDto(dto);
            return _repo.CreateAsync(userId, dto);
        }

        public Task<UserAddressDto?> UpdateAsync(long id, long userId, UserAddressUpsertDto dto)
        {
            ValidateDto(dto);
            return _repo.UpdateAsync(id, userId, dto);
        }

        public Task<bool> DeleteAsync(long id, long userId)
        {
            return _repo.DeleteAsync(id, userId);
        }

        public Task<bool> SetDefaultAsync(long id, long userId)
        {
            return _repo.SetDefaultAsync(id, userId);
        }

        private static void ValidateDto(UserAddressUpsertDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullAddress))
            {
                throw new ArgumentException("Full address is required");
            }
        }
    }
}
