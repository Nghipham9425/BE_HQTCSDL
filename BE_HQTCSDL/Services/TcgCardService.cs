using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Services
{
    public class TcgCardService : ITcgCardService
    {
        private readonly ITcgCardRepository _repo;
        private readonly ApplicationDbContext _db; // Assuming we inject db for validation

        public TcgCardService(ITcgCardRepository repo, ApplicationDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public Task<TcgCardPagedResponseDto> GetPagedAsync(string? setId, string? q, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            return _repo.GetPagedAsync(setId, q, page, pageSize);
        }

        public Task<TcgCardDetailDto?> GetByIdAsync(string cardId) => _repo.GetByIdAsync(cardId);

        public Task<List<TcgCardListItemDto>> GetBySetAsync(string setId) => _repo.GetBySetAsync(setId);

        public async Task<TcgCardDetailDto> CreateAsync(TcgCardUpsertDto dto)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(dto.CardId)) throw new ArgumentException("CardId is required");
            if (string.IsNullOrWhiteSpace(dto.SetId)) throw new ArgumentException("SetId is required");
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required");
            if (string.IsNullOrWhiteSpace(dto.CardNumber)) throw new ArgumentException("CardNumber is required");

            // Check if Set exists
            var setExists = await _db.TcgSets.AnyAsync(s => s.SetId == dto.SetId);
            if (!setExists) throw new ArgumentException("Invalid SetId");

            // Check CardId unique
            var cardExists = await _db.TcgCards.AnyAsync(c => c.CardId == dto.CardId);
            if (cardExists) throw new ArgumentException("CardId already exists");

            return await _repo.CreateAsync(dto);
        }

        public async Task<TcgCardDetailDto?> UpdateAsync(string cardId, TcgCardUpsertDto dto)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(dto.SetId)) throw new ArgumentException("SetId is required");
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required");
            if (string.IsNullOrWhiteSpace(dto.CardNumber)) throw new ArgumentException("CardNumber is required");

            // Check if Set exists
            var setExists = await _db.TcgSets.AnyAsync(s => s.SetId == dto.SetId);
            if (!setExists) throw new ArgumentException("Invalid SetId");

            return await _repo.UpdateAsync(cardId, dto);
        }

        public Task<bool> DeleteAsync(string cardId) => _repo.DeleteAsync(cardId);
    }
}