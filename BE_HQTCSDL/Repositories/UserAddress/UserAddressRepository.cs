using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Repositories
{
    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly ApplicationDbContext _db;

        public UserAddressRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserAddressDto>> GetByUserIdAsync(long userId)
        {
            return await _db.UserAddresses
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.UpdatedAt)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<UserAddressDto?> GetByIdAsync(long id, long userId)
        {
            var address = await _db.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            return address != null ? MapToDto(address) : null;
        }

        public async Task<UserAddressDto?> GetDefaultAsync(long userId)
        {
            var address = await _db.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault == 1);

            return address != null ? MapToDto(address) : null;
        }

        public async Task<UserAddressDto> CreateAsync(long userId, UserAddressUpsertDto dto)
        {
            // Clear default before inserting to avoid trigger conflict
            if (dto.IsDefault)
            {
                await ClearDefaultAsync(userId);
                await _db.SaveChangesAsync();
            }

            var address = new UserAddress
            {
                UserId = userId,
                AddressName = dto.AddressName?.Trim(),
                RecipientName = dto.RecipientName?.Trim(),
                FullAddress = dto.FullAddress.Trim(),
                City = dto.City?.Trim(),
                District = dto.District?.Trim(),
                Ward = dto.Ward?.Trim(),
                PostalCode = dto.PostalCode?.Trim(),
                Country = dto.Country?.Trim() ?? "Vietnam",
                Phone = dto.Phone?.Trim(),
                IsDefault = dto.IsDefault ? 1 : 0
                // CreatedAt/UpdatedAt are auto-set by database DEFAULT and trigger
            };

            _db.UserAddresses.Add(address);
            await _db.SaveChangesAsync();

            // Reload to get database-generated values
            await _db.Entry(address).ReloadAsync();
            return MapToDto(address);
        }

        public async Task<UserAddressDto?> UpdateAsync(long id, long userId, UserAddressUpsertDto dto)
        {
            var address = await _db.UserAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (address == null) return null;

            // Clear default before updating to avoid trigger conflict
            if (dto.IsDefault && address.IsDefault != 1)
            {
                await ClearDefaultAsync(userId);
                await _db.SaveChangesAsync();
            }

            address.AddressName = dto.AddressName?.Trim();
            address.RecipientName = dto.RecipientName?.Trim();
            address.FullAddress = dto.FullAddress.Trim();
            address.City = dto.City?.Trim();
            address.District = dto.District?.Trim();
            address.Ward = dto.Ward?.Trim();
            address.PostalCode = dto.PostalCode?.Trim();
            address.Country = dto.Country?.Trim() ?? "Vietnam";
            address.Phone = dto.Phone?.Trim();
            address.IsDefault = dto.IsDefault ? 1 : 0;
            // UpdatedAt is auto-set by TRG_USER_ADDRESSES_UPDATED_AT trigger

            await _db.SaveChangesAsync();

            // Reload to get trigger-updated values
            await _db.Entry(address).ReloadAsync();
            return MapToDto(address);
        }

        public async Task<bool> DeleteAsync(long id, long userId)
        {
            var address = await _db.UserAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (address == null) return false;

            _db.UserAddresses.Remove(address);
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetDefaultAsync(long id, long userId)
        {
            var address = await _db.UserAddresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (address == null) return false;

            // Clear other defaults before setting new one to avoid trigger conflict
            await ClearDefaultAsync(userId);
            await _db.SaveChangesAsync();

            address.IsDefault = 1;
            // UpdatedAt is auto-set by TRG_USER_ADDRESSES_UPDATED_AT trigger
            await _db.SaveChangesAsync();

            return true;
        }

        private async Task ClearDefaultAsync(long userId)
        {
            var currentDefault = await _db.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault == 1)
                .ToListAsync();

            foreach (var addr in currentDefault)
            {
                addr.IsDefault = 0;
            }
        }

        private static UserAddressDto MapToDto(UserAddress address)
        {
            return new UserAddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                AddressName = address.AddressName,
                RecipientName = address.RecipientName,
                FullAddress = address.FullAddress,
                City = address.City,
                District = address.District,
                Ward = address.Ward,
                PostalCode = address.PostalCode,
                Country = address.Country,
                Phone = address.Phone,
                IsDefault = address.IsDefault == 1,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt
            };
        }
    }
}
