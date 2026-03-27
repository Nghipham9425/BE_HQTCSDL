using System;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly ApplicationDbContext _db;

        public CategoryService(ICategoryRepository repo, ApplicationDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public Task<CategoryPagedResponseDto> GetPagedAsync(string? q, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            return _repo.GetPagedAsync(q, page, pageSize);
        }

        public Task<CategoryDetailDto?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);

        public async Task<CategoryDetailDto> CreateAsync(CategoryUpsertRequestDto dto)
        {
            await ValidateAsync(dto, null);
            return await _repo.CreateAsync(dto);
        }

        public async Task<CategoryDetailDto?> UpdateAsync(long id, CategoryUpsertRequestDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            await ValidateAsync(dto, id);
            return await _repo.UpdateAsync(id, dto);
        }

        public Task<bool> DeleteAsync(long id) => _repo.DeleteAsync(id);

        private async Task ValidateAsync(CategoryUpsertRequestDto dto, long? categoryId)
        {
            if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required");

            dto.Name = dto.Name.Trim();

            var normalizedName = dto.Name.ToLower();

            var query = _db.Categories.Where(c => c.Name.ToLower() == normalizedName);
            if (categoryId.HasValue)
            {
                query = query.Where(c => c.Id != categoryId.Value);
            }

            var nameExists = await query.CountAsync() > 0;

            if (nameExists) throw new ArgumentException("Category name already exists");
        }
    }
}
