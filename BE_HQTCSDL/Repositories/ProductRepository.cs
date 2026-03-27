using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE_HQTCSDL.Database;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ProductPagedResponseDto> GetPagedAsync(
            string? q,
            string? productType,
            bool? isActive,
            long? categoryId,
            string? categoryName,
            int page,
            int pageSize)
        {
            var query = _db.Products
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    p.Sku.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(productType))
            {
                var normalizedType = productType.Trim().ToUpper();
                query = query.Where(p => p.ProductType == normalizedType);
            }

            if (isActive.HasValue)
            {
                var activeValue = isActive.Value ? 1 : 0;
                query = query.Where(p => p.IsActive == activeValue);
            }

            if (categoryId.HasValue)
            {
                var requestedCategoryId = categoryId.Value;
                query = query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == requestedCategoryId));
            }

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var keyword = categoryName.Trim().ToLower();
                query = query.Where(p =>
                    p.ProductCategories.Any(pc => pc.Category.Name.ToLower().Contains(keyword)));
            }

            var total = await query.CountAsync();

            var rows = await query
                .OrderByDescending(p => p.UpdatedAt)
                .ThenByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    ProductType = p.ProductType,
                    Price = p.Price,
                    Stock = p.Stock,
                    IsActiveValue = p.IsActive,
                    Thumbnail = p.Thumbnail,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            var items = rows.Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                ProductType = p.ProductType,
                Price = p.Price,
                Stock = p.Stock,
                IsActive = p.IsActiveValue == 1,
                Thumbnail = p.Thumbnail,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            return new ProductPagedResponseDto
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }

        public async Task<ProductDetailDto?> GetByIdAsync(long id)
        {
            var row = await _db.Products
                .AsNoTracking()
                .Include(p => p.ProductCategories)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    ProductType = p.ProductType,
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    Weight = p.Weight,
                    Descriptions = p.Descriptions,
                    Thumbnail = p.Thumbnail,
                    Image = p.Image,
                    Stock = p.Stock,
                    IsActiveValue = p.IsActive,
                    CardId = p.CardId,
                    CategoryIds = p.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                    CreateDate = p.CreateDate,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (row == null) return null;

            return new ProductDetailDto
            {
                Id = row.Id,
                Sku = row.Sku,
                Name = row.Name,
                ProductType = row.ProductType,
                Price = row.Price,
                OriginalPrice = row.OriginalPrice,
                Weight = row.Weight,
                Descriptions = row.Descriptions,
                Thumbnail = row.Thumbnail,
                Image = row.Image,
                Stock = row.Stock,
                IsActive = row.IsActiveValue == 1,
                CardId = row.CardId,
                CategoryIds = row.CategoryIds,
                CreateDate = row.CreateDate,
                UpdatedAt = row.UpdatedAt
            };
        }

        public async Task<ProductDetailDto> CreateAsync(ProductUpsertRequestDto dto)
        {
            var now = DateTime.Now;
            var product = new Product
            {
                Sku = dto.Sku.Trim(),
                Name = dto.Name.Trim(),
                ProductType = dto.ProductType,
                Price = dto.Price,
                OriginalPrice = dto.OriginalPrice,
                Weight = dto.Weight,
                Descriptions = dto.Descriptions,
                Thumbnail = dto.Thumbnail,
                Image = dto.Image,
                Stock = dto.Stock,
                IsActive = dto.IsActive ? 1 : 0,
                CardId = dto.CardId,
                CreateDate = now,
                UpdatedAt = now
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            if (dto.CategoryIds.Count > 0)
            {
                var categoryRows = dto.CategoryIds
                    .Distinct()
                    .Select(categoryId => new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId
                    });

                _db.ProductCategories.AddRange(categoryRows);
            }

            await _db.SaveChangesAsync();

            return await GetByIdAsync(product.Id) ?? throw new InvalidOperationException("Failed to create product");
        }

        public async Task<ProductDetailDto?> UpdateAsync(long id, ProductUpsertRequestDto dto)
        {
            var product = await _db.Products
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            product.Sku = dto.Sku.Trim();
            product.Name = dto.Name.Trim();
            product.ProductType = dto.ProductType;
            product.Price = dto.Price;
            product.OriginalPrice = dto.OriginalPrice;
            product.Weight = dto.Weight;
            product.Descriptions = dto.Descriptions;
            product.Thumbnail = dto.Thumbnail;
            product.Image = dto.Image;
            product.Stock = dto.Stock;
            product.IsActive = dto.IsActive ? 1 : 0;
            product.CardId = dto.CardId;
            product.UpdatedAt = DateTime.Now;

            _db.ProductCategories.RemoveRange(product.ProductCategories);

            if (dto.CategoryIds.Count > 0)
            {
                var categoryRows = dto.CategoryIds
                    .Distinct()
                    .Select(categoryId => new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId
                    });

                _db.ProductCategories.AddRange(categoryRows);
            }

            await _db.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            product.IsActive = 0;
            product.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}