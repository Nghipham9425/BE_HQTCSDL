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
    public class TcgSetRepository : ITcgSetRepository
    {
        private readonly ApplicationDbContext _db;

        public TcgSetRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<TcgSetPagedResponseDto> GetPagedAsync(string? q, int page, int pageSize)
        {
            var query = _db.TcgSets
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim().ToLower();
                query = query.Where(s =>
                    s.SetId.ToLower().Contains(keyword) ||
                    s.Name.ToLower().Contains(keyword) ||
                    (s.Series != null && s.Series.ToLower().Contains(keyword)));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.ReleaseDate)
                .ThenBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new TcgSetListItemDto
                {
                    SetId = s.SetId,
                    Name = s.Name,
                    Series = s.Series,
                    ReleaseDate = s.ReleaseDate,
                    TotalCards = s.TotalCards,
                    LogoUrl = s.LogoUrl,
                    CardCount = s.Cards.Count
                })
                .ToListAsync();

            return new TcgSetPagedResponseDto
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<TcgSetDetailDto?> GetByIdAsync(string setId)
        {
            return await _db.TcgSets
                .AsNoTracking()
                .Where(s => s.SetId == setId)
                .Select(s => new TcgSetDetailDto
                {
                    SetId = s.SetId,
                    Name = s.Name,
                    Series = s.Series,
                    ReleaseDate = s.ReleaseDate,
                    TotalCards = s.TotalCards,
                    LogoUrl = s.LogoUrl,
                    CardCount = s.Cards.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TcgSetDetailDto> CreateAsync(TcgSetUpsertDto dto)
        {
            var entity = new TcgSet
            {
                SetId = dto.SetId.Trim(),
                Name = dto.Name.Trim(),
                Series = dto.Series,
                ReleaseDate = dto.ReleaseDate,
                TotalCards = dto.TotalCards,
                LogoUrl = dto.LogoUrl
            };

            _db.TcgSets.Add(entity);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(entity.SetId) ?? throw new InvalidOperationException("Failed to create tcg set");
        }

        public async Task<TcgSetDetailDto?> UpdateAsync(string setId, TcgSetUpsertDto dto)
        {
            var entity = await _db.TcgSets.FirstOrDefaultAsync(s => s.SetId == setId);
            if (entity == null) return null;

            entity.Name = dto.Name.Trim();
            entity.Series = dto.Series;
            entity.ReleaseDate = dto.ReleaseDate;
            entity.TotalCards = dto.TotalCards;
            entity.LogoUrl = dto.LogoUrl;

            await _db.SaveChangesAsync();
            return await GetByIdAsync(setId);
        }

        public async Task<bool> DeleteAsync(string setId)
        {
            var entity = await _db.TcgSets
                .Include(s => s.Cards)
                .FirstOrDefaultAsync(s => s.SetId == setId);

            if (entity == null) return false;
            if (entity.Cards.Any()) return false;

            _db.TcgSets.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
