using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Data;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Repositories
{
	public class TcgCardRepository : ITcgCardRepository
	{
		private readonly ApplicationDbContext _db;

		public TcgCardRepository(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<TcgCardPagedResponseDto> GetPagedAsync(string? setId, string? q, int page, int pageSize)
		{
			var query = _db.TcgCards
				.AsNoTracking()
				.Include(c => c.Set)
				.Where(c => !c.IsDeleted)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(setId))
			{
				var normalizedSetId = setId.Trim();
				query = query.Where(c => c.SetId == normalizedSetId);
			}

			if (!string.IsNullOrWhiteSpace(q))
			{
				var keyword = q.Trim().ToLower();
				query = query.Where(c =>
					c.Name.ToLower().Contains(keyword) ||
					c.CardNumber.ToLower().Contains(keyword));
			}

			var total = await query.CountAsync();

			var items = await query
				.OrderBy(c => c.SetId)
				.ThenBy(c => c.CardNumber)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(c => new TcgCardListItemDto
				{
					CardId = c.CardId,
					SetId = c.SetId,
					SetName = c.Set.Name,
					Name = c.Name,
					CardNumber = c.CardNumber,
					Rarity = c.Rarity,
					ImageSmall = c.ImageSmall,
					ImageLarge = c.ImageLarge
				})
				.ToListAsync();

			return new TcgCardPagedResponseDto
			{
				Page = page,
				PageSize = pageSize,
				Total = total,
				Items = items
			};
		}

		public async Task<TcgCardDetailDto?> GetByIdAsync(string cardId)
		{
			return await _db.TcgCards
				.AsNoTracking()
				.Include(c => c.Set)
				.Where(c => c.CardId == cardId && !c.IsDeleted)
				.Select(c => new TcgCardDetailDto
				{
					CardId = c.CardId,
					SetId = c.SetId,
					SetName = c.Set.Name,
					Name = c.Name,
					CardNumber = c.CardNumber,
					Rarity = c.Rarity,
					ImageSmall = c.ImageSmall,
					ImageLarge = c.ImageLarge,
					Series = c.Set.Series,
					ReleaseDate = c.Set.ReleaseDate
				})
				.FirstOrDefaultAsync();
		}

		public async Task<List<TcgCardListItemDto>> GetBySetAsync(string setId)
		{
			return await _db.TcgCards
				.AsNoTracking()
				.Include(c => c.Set)
				.Where(c => c.SetId == setId && !c.IsDeleted)
				.OrderBy(c => c.CardNumber)
				.Select(c => new TcgCardListItemDto
				{
					CardId = c.CardId,
					SetId = c.SetId,
					SetName = c.Set.Name,
					Name = c.Name,
					CardNumber = c.CardNumber,
					Rarity = c.Rarity,
					ImageSmall = c.ImageSmall,
					ImageLarge = c.ImageLarge
				})
				.ToListAsync();
		}

		public async Task<TcgCardDetailDto> CreateAsync(TcgCardUpsertDto dto)
		{
			var card = new TcgCard
			{
				CardId = dto.CardId,
				SetId = dto.SetId,
				Name = dto.Name,
				CardNumber = dto.CardNumber,
				Rarity = dto.Rarity,
				ImageSmall = dto.ImageSmall,
				ImageLarge = dto.ImageLarge,
				IsDeleted = false
			};

			_db.TcgCards.Add(card);
			await _db.SaveChangesAsync();

			return await GetByIdAsync(dto.CardId) ?? throw new InvalidOperationException("Failed to create card");
		}

		public async Task<TcgCardDetailDto?> UpdateAsync(string cardId, TcgCardUpsertDto dto)
		{
			var card = await _db.TcgCards.FirstOrDefaultAsync(c => c.CardId == cardId && !c.IsDeleted);
			if (card == null) return null;

			card.SetId = dto.SetId;
			card.Name = dto.Name;
			card.CardNumber = dto.CardNumber;
			card.Rarity = dto.Rarity;
			card.ImageSmall = dto.ImageSmall;
			card.ImageLarge = dto.ImageLarge;

			await _db.SaveChangesAsync();

			return await GetByIdAsync(cardId);
		}

		public async Task<bool> DeleteAsync(string cardId)
		{
			var card = await _db.TcgCards.FirstOrDefaultAsync(c => c.CardId == cardId && !c.IsDeleted);
			if (card == null) return false;

			card.IsDeleted = true;
			await _db.SaveChangesAsync();
			return true;
		}
	}
}
