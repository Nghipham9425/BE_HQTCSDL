using System;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IPaymentMethodRepository _repo;
        private readonly ApplicationDbContext _db;

        public PaymentMethodService(IPaymentMethodRepository repo, ApplicationDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public Task<PaymentMethodPagedResponseDto> GetPagedAsync(string? q, bool? isActive, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            return _repo.GetPagedAsync(q, isActive, page, pageSize);
        }

        public Task<PaymentMethodDetailDto?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);

        public async Task<PaymentMethodDetailDto> CreateAsync(PaymentMethodUpsertDto dto)
        {
            await ValidateAsync(dto, null);
            return await _repo.CreateAsync(dto);
        }

        public async Task<PaymentMethodDetailDto?> UpdateAsync(long id, PaymentMethodUpsertDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            await ValidateAsync(dto, id);
            return await _repo.UpdateAsync(id, dto);
        }

        public Task<bool> DeleteAsync(long id) => _repo.DeleteAsync(id);

        private async Task ValidateAsync(PaymentMethodUpsertDto dto, long? methodId)
        {
            if (string.IsNullOrWhiteSpace(dto.MethodName)) throw new ArgumentException("MethodName is required");

            dto.MethodName = dto.MethodName.Trim();

            var normalizedName = dto.MethodName.ToLower();

            var query = _db.PaymentMethods.Where(m => m.MethodName.ToLower() == normalizedName);
            if (methodId.HasValue)
            {
                query = query.Where(m => m.Id != methodId.Value);
            }

            var nameExists = await query.CountAsync() > 0;

            if (nameExists) throw new ArgumentException("MethodName already exists");
        }
    }
}
