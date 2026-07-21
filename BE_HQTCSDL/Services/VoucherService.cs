using System;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;
using BE_HQTCSDL.Utils;
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
            (page, pageSize) = Pagination.Normalize(page, pageSize);

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

        public async Task<VoucherPreviewResponseDto> PreviewDiscountAsync(long amount, string voucherCode)
        {
            if (amount <= 0) throw new ArgumentException("Order amount must be greater than 0");
            if (string.IsNullOrWhiteSpace(voucherCode)) throw new ArgumentException("Voucher code is required");

            var normalizedCode = voucherCode.Trim().ToUpper();
            var now = DateTime.Now;

            var voucher = await _db.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v =>
                    v.Code == normalizedCode &&
                    v.IsActive == 1 &&
                    v.StartDate <= now &&
                    v.EndDate >= now);

            if (voucher == null) throw new ArgumentException("Voucher không hợp lệ hoặc đã hết hạn");

            if (amount < voucher.MinOrderValue)
            {
                throw new ArgumentException("Đơn hàng chưa đạt giá trị tối thiểu để áp dụng voucher");
            }

            if (voucher.UsageLimit.HasValue)
            {
                var usedCount = await _db.Orders.CountAsync(o => o.VoucherId == voucher.Id);
                if (usedCount >= voucher.UsageLimit.Value)
                {
                    throw new ArgumentException("Voucher đã hết lượt sử dụng");
                }
            }

            long discount;
            if (voucher.DiscountType == "PERCENT")
            {
                discount = amount * voucher.DiscountValue / 100;
                if (voucher.MaxDiscount.HasValue && discount > voucher.MaxDiscount.Value)
                {
                    discount = voucher.MaxDiscount.Value;
                }
            }
            else if (voucher.DiscountType == "FIXED")
            {
                discount = voucher.DiscountValue;
            }
            else
            {
                throw new ArgumentException("Loại giảm giá voucher không được hỗ trợ");
            }

            if (discount > amount) discount = amount;

            return new VoucherPreviewResponseDto
            {
                OriginalAmount = amount,
                Discount = discount,
                DiscountedAmount = amount - discount,
                VoucherCode = normalizedCode
            };
        }

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
