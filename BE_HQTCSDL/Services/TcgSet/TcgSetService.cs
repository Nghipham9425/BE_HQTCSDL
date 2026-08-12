using System;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;
using BE_HQTCSDL.Utils;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Services
{
    public class TcgSetService : ITcgSetService
    {
        private readonly ITcgSetRepository _repo;
        private readonly ApplicationDbContext _db;

        public TcgSetService(ITcgSetRepository repo, ApplicationDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public Task<TcgSetPagedResponseDto> GetPagedAsync(string? q, int page, int pageSize)
        {
            (page, pageSize) = Pagination.Normalize(page, pageSize);

            return _repo.GetPagedAsync(q, page, pageSize);
        }

        public Task<TcgSetDetailDto?> GetByIdAsync(string setId)
        {
            if (string.IsNullOrWhiteSpace(setId)) throw new ArgumentException("SetId is required");
            return _repo.GetByIdAsync(setId.Trim());
        }

        public async Task<TcgSetDetailDto> CreateAsync(TcgSetUpsertDto dto)
        {
            await ValidateAsync(dto, false, null);
            return await _repo.CreateAsync(dto);
        }

        public async Task<TcgSetDetailDto?> UpdateAsync(string setId, TcgSetUpsertDto dto)
        {
            if (string.IsNullOrWhiteSpace(setId)) throw new ArgumentException("SetId is required");

            var normalizedSetId = setId.Trim();
            var existing = await _repo.GetByIdAsync(normalizedSetId);
            if (existing == null) return null;

            await ValidateAsync(dto, true, normalizedSetId);
            return await _repo.UpdateAsync(normalizedSetId, dto);
        }

        public Task<bool> DeleteAsync(string setId)
        {
            if (string.IsNullOrWhiteSpace(setId)) throw new ArgumentException("SetId is required");
            return _repo.DeleteAsync(setId.Trim());
        }

        private async Task ValidateAsync(TcgSetUpsertDto dto, bool isUpdate, string? currentSetId)
        {
            if (dto == null) throw new ArgumentException("Payload is required");
            if (string.IsNullOrWhiteSpace(dto.SetId)) throw new ArgumentException("SetId is required");
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required");

            dto.SetId = dto.SetId.Trim();
            dto.Name = dto.Name.Trim();
            dto.Series = string.IsNullOrWhiteSpace(dto.Series) ? null : dto.Series.Trim();
            dto.LogoUrl = string.IsNullOrWhiteSpace(dto.LogoUrl) ? null : dto.LogoUrl.Trim();

            if (dto.SetId.Length > 20) throw new ArgumentException("SetId cannot exceed 20 characters");
            if (dto.Name.Length > 100) throw new ArgumentException("Name cannot exceed 100 characters");
            if (dto.Series?.Length > 100) throw new ArgumentException("Series cannot exceed 100 characters");
            if (dto.LogoUrl?.Length > 500) throw new ArgumentException("LogoUrl cannot exceed 500 characters");

            if (dto.TotalCards.HasValue && dto.TotalCards.Value < 0)
            {
                throw new ArgumentException("TotalCards must be greater than or equal to 0");
            }

            if (!isUpdate)
            {
                var setIdExists = await _db.TcgSets.CountAsync(s => s.SetId == dto.SetId) > 0;
                if (setIdExists) throw new ArgumentException("SetId already exists");
            }
            else if (!string.Equals(dto.SetId, currentSetId, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("SetId cannot be changed");
            }
        }
    }
}
