using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using BE_HQTCSDL.Database.connection;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Models;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db)
    {
        this._db = db;
    }
    public async Task<CategoryPagedResponseDto> GetPagedAsync(string? q, int page, int pageSize)
    {
        var query = _db.Categories
                    .AsNoTracking()
                    .AsQueryable();
        if (!string.IsNullOrEmpty(q))
        {
            var keyword = q.Trim().ToLower();
            query = query.Where(c =>
            c.Name.ToLower().Contains(keyword));
        }

        var total = await query.CountAsync();

        var items = await query
        .OrderBy(c => c.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new CategoryListItemDto
        {
            Id = c.Id,
            Name = c.Name,
            Thumbnail = c.Thumbnail,
            ProductCount = c.ProductCategories.Count
        }).ToListAsync();

        return new CategoryPagedResponseDto
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<CategoryDetailDto?> GetByIdAsync(long id)
    {
        return await _db.Categories
        .AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => new CategoryDetailDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Thumbnail = c.Thumbnail,
            ProductCount = c.ProductCategories.Count
        })
        .FirstOrDefaultAsync();
    }

    public async Task<CategoryDetailDto> CreateAsync(CategoryUpsertRequestDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            Thumbnail = dto.Thumbnail
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(category.Id) ?? throw new InvalidOperationException("failed to create category");
    }

    public async Task<CategoryDetailDto?> UpdateAsync(long id, CategoryUpsertRequestDto dto)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return null;

        category.Name = dto.Name.Trim();
        category.Description = dto.Description;
        category.Thumbnail = dto.Thumbnail;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var category = await _db.Categories
        .Include(c => c.ProductCategories)
        .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;

        if (category.ProductCategories.Any()) return false;

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return true;
    }

}