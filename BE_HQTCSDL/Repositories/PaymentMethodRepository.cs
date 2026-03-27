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
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly ApplicationDbContext _db;

        public PaymentMethodRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PaymentMethodPagedResponseDto> GetPagedAsync(string? q, bool? isActive, int page, int pageSize)
        {
            var query = _db.PaymentMethods
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim().ToLower();
                query = query.Where(m => m.MethodName.ToLower().Contains(keyword));
            }

            if (isActive.HasValue)
            {
                var activeValue = isActive.Value ? 1 : 0;
                query = query.Where(m => m.IsActive == activeValue);
            }

            var total = await query.CountAsync();

            var rows = await query
                .OrderBy(m => m.MethodName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    Id = m.Id,
                    MethodName = m.MethodName,
                    IsActiveValue = m.IsActive,
                    PaymentCount = m.Payments.Count
                })
                .ToListAsync();

            var items = rows.Select(m => new PaymentMethodListItemDto
            {
                Id = m.Id,
                MethodName = m.MethodName,
                IsActive = m.IsActiveValue == 1,
                PaymentCount = m.PaymentCount
            }).ToList();

            return new PaymentMethodPagedResponseDto
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<PaymentMethodDetailDto?> GetByIdAsync(long id)
        {
            var row = await _db.PaymentMethods
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new
                {
                    Id = m.Id,
                    MethodName = m.MethodName,
                    IsActiveValue = m.IsActive,
                    PaymentCount = m.Payments.Count
                })
                .FirstOrDefaultAsync();

            if (row == null) return null;

            return new PaymentMethodDetailDto
            {
                Id = row.Id,
                MethodName = row.MethodName,
                IsActive = row.IsActiveValue == 1,
                PaymentCount = row.PaymentCount
            };
        }

        public async Task<PaymentMethodDetailDto> CreateAsync(PaymentMethodUpsertDto dto)
        {
            var entity = new PaymentMethod
            {
                MethodName = dto.MethodName.Trim(),
                IsActive = dto.IsActive ? 1 : 0
            };

            _db.PaymentMethods.Add(entity);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(entity.Id) ?? throw new InvalidOperationException("Failed to create payment method");
        }

        public async Task<PaymentMethodDetailDto?> UpdateAsync(long id, PaymentMethodUpsertDto dto)
        {
            var entity = await _db.PaymentMethods.FirstOrDefaultAsync(m => m.Id == id);
            if (entity == null) return null;

            entity.MethodName = dto.MethodName.Trim();
            entity.IsActive = dto.IsActive ? 1 : 0;

            await _db.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _db.PaymentMethods
                .Include(m => m.Payments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity == null) return false;
            if (entity.Payments.Any()) return false;

            _db.PaymentMethods.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
