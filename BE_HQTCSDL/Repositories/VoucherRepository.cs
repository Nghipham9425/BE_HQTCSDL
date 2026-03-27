using System;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Repositories
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly ApplicationDbContext _db;

        public VoucherRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<VoucherPagedResponseDto> GetPagedAsync(string? q, bool? isActive, int page, int pageSize)
        {
            var query = _db.Vouchers
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim().ToLower();
                query = query.Where(v =>
                    v.Code.ToLower().Contains(keyword) ||
                    v.Name.ToLower().Contains(keyword));
            }

            if (isActive.HasValue)
            {
                var activeValue = isActive.Value ? 1 : 0;
                query = query.Where(v => v.IsActive == activeValue);
            }

            var total = await query.CountAsync();

            var rows = await query
                .OrderByDescending(v => v.EndDate)
                .ThenBy(v => v.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new
                {
                    Id = v.Id,
                    Code = v.Code,
                    Name = v.Name,
                    DiscountType = v.DiscountType,
                    DiscountValue = v.DiscountValue,
                    MinOrderValue = v.MinOrderValue,
                    MaxDiscount = v.MaxDiscount,
                    UsageLimit = v.UsageLimit,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    IsActiveValue = v.IsActive
                })
                .ToListAsync();

            var items = rows.Select(v => new VoucherListItemDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                DiscountType = v.DiscountType,
                DiscountValue = v.DiscountValue,
                MinOrderValue = v.MinOrderValue,
                MaxDiscount = v.MaxDiscount,
                UsageLimit = v.UsageLimit,
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                IsActive = v.IsActiveValue == 1
            }).ToList();

            return new VoucherPagedResponseDto
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<VoucherDetailDto?> GetByIdAsync(long id)
        {
            var row = await _db.Vouchers
                .AsNoTracking()
                .Where(v => v.Id == id)
                .Select(v => new
                {
                    Id = v.Id,
                    Code = v.Code,
                    Name = v.Name,
                    DiscountType = v.DiscountType,
                    DiscountValue = v.DiscountValue,
                    MinOrderValue = v.MinOrderValue,
                    MaxDiscount = v.MaxDiscount,
                    UsageLimit = v.UsageLimit,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    IsActiveValue = v.IsActive,
                    UsedCount = v.Orders.Count
                })
                .FirstOrDefaultAsync();

            if (row == null) return null;

            return new VoucherDetailDto
            {
                Id = row.Id,
                Code = row.Code,
                Name = row.Name,
                DiscountType = row.DiscountType,
                DiscountValue = row.DiscountValue,
                MinOrderValue = row.MinOrderValue,
                MaxDiscount = row.MaxDiscount,
                UsageLimit = row.UsageLimit,
                StartDate = row.StartDate,
                EndDate = row.EndDate,
                IsActive = row.IsActiveValue == 1,
                UsedCount = row.UsedCount
            };
        }

        public async Task<VoucherDetailDto> CreateAsync(VoucherUpsertDto dto)
        {
            var voucher = new Voucher
            {
                Code = dto.Code.Trim().ToUpper(),
                Name = dto.Name.Trim(),
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinOrderValue = dto.MinOrderValue,
                MaxDiscount = dto.MaxDiscount,
                UsageLimit = dto.UsageLimit,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive ? 1 : 0
            };

            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(voucher.Id) ?? throw new InvalidOperationException("Failed to create voucher");
        }

        public async Task<VoucherDetailDto?> UpdateAsync(long id, VoucherUpsertDto dto)
        {
            var voucher = await _db.Vouchers.FirstOrDefaultAsync(v => v.Id == id);
            if (voucher == null) return null;

            voucher.Code = dto.Code.Trim().ToUpper();
            voucher.Name = dto.Name.Trim();
            voucher.DiscountType = dto.DiscountType;
            voucher.DiscountValue = dto.DiscountValue;
            voucher.MinOrderValue = dto.MinOrderValue;
            voucher.MaxDiscount = dto.MaxDiscount;
            voucher.UsageLimit = dto.UsageLimit;
            voucher.StartDate = dto.StartDate;
            voucher.EndDate = dto.EndDate;
            voucher.IsActive = dto.IsActive ? 1 : 0;

            await _db.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var voucher = await _db.Vouchers
                .Include(v => v.Orders)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (voucher == null) return false;
            if (voucher.Orders.Any()) return false;

            _db.Vouchers.Remove(voucher);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
