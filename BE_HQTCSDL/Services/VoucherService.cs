using System;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Services
{
    public class VoucherService : IVoucherService
    {
        private static readonly string[] AllowedDiscountTypes = { "PERCENT", "FIXED" };

        private readonly IVoucherRepository _repo;
        private readonly ApplicationDbContext _db;

        public VoucherService(IVoucherRepository repo, ApplicationDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public Task<VoucherPagedResponseDto> GetPagedAsync(string? q, bool? isActive, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            return _repo.GetPagedAsync(q, isActive, page, pageSize);
        }

        public Task<VoucherDetailDto?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);

        public async Task<VoucherDetailDto> CreateAsync(VoucherUpsertDto dto)
        {
            await ValidateAsync(dto, null);
            return await _repo.CreateAsync(dto);
        }

        public async Task<VoucherDetailDto?> UpdateAsync(long id, VoucherUpsertDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            await ValidateAsync(dto, id);
            return await _repo.UpdateAsync(id, dto);
        }

        public Task<bool> DeleteAsync(long id) => _repo.DeleteAsync(id);

        private async Task ValidateAsync(VoucherUpsertDto dto, long? voucherId)
        {
            if (string.IsNullOrWhiteSpace(dto.Code)) throw new ArgumentException("Code is required");
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required");
            if (string.IsNullOrWhiteSpace(dto.DiscountType)) throw new ArgumentException("DiscountType is required");

            dto.Code = dto.Code.Trim().ToUpper();
            dto.Name = dto.Name.Trim();
            dto.DiscountType = dto.DiscountType.Trim().ToUpper();

            if (!AllowedDiscountTypes.Contains(dto.DiscountType))
            {
                throw new ArgumentException("Invalid DiscountType");
            }

            if (dto.DiscountValue <= 0)
            {
                throw new ArgumentException("DiscountValue must be greater than 0");
            }

            if (dto.DiscountType == "PERCENT" && dto.DiscountValue > 100)
            {
                throw new ArgumentException("Percent discount cannot exceed 100");
            }

            if (dto.MinOrderValue < 0)
            {
                throw new ArgumentException("MinOrderValue must be greater than or equal to 0");
            }

            if (dto.MaxDiscount.HasValue && dto.MaxDiscount.Value <= 0)
            {
                throw new ArgumentException("MaxDiscount must be greater than 0");
            }

            if (dto.UsageLimit.HasValue && dto.UsageLimit.Value <= 0)
            {
                throw new ArgumentException("UsageLimit must be greater than 0");
            }

            if (dto.EndDate < dto.StartDate)
            {
                throw new ArgumentException("EndDate must be greater than or equal to StartDate");
            }

            var query = _db.Vouchers.Where(v => v.Code.ToUpper() == dto.Code);
            if (voucherId.HasValue)
            {
                query = query.Where(v => v.Id != voucherId.Value);
            }

            var codeExists = await query.CountAsync() > 0;

            if (codeExists) throw new ArgumentException("Code already exists");
        }
    }
}
